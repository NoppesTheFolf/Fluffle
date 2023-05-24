using Flurl.Http;
using Nito.AsyncEx;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Noppes.Fluffle.B2;

internal class B2BucketUploadClient
{
    /// <summary>
    /// The amount of time after which an upload token will be considered due for a refresh. The
    /// token expired every 24 hours, so picking 23 hours is a safe bet.
    /// </summary>
    private static readonly TimeSpan TokenExpirationInterval = TimeSpan.FromHours(23);

    private readonly B2Client _client;
    private readonly B2Bucket _bucket;
    private readonly AsyncLock _mutex;

    private DateTimeOffset _authorizedWhen;
    private string _uploadUrl;
    private string _uploadAuthorizationToken;

    public B2BucketUploadClient(B2Client client, B2Bucket bucket)
    {
        _authorizedWhen = DateTimeOffset.MinValue;
        _mutex = new AsyncLock();

        _client = client;
        _bucket = bucket;
    }

    /// <summary>
    /// Uploads a stream of data to the associated bucket of this instance at the given location.
    /// </summary>
    public async Task<B2UploadResponse> UploadAsync(Func<Stream> openStream, string fileLocation, string contentType, params KeyValuePair<string, string>[] info)
    {
        string sha1;
        await using (var stream = openStream())
        {
            sha1 = Hashing.Sha1(stream);
        }

        var isFirstAttempt = true;
        while (true)
        {
            try
            {
                await using var stream = openStream();

                await RefreshAuthorizationAsync(!isFirstAttempt);

                var request = _client.Request(_uploadUrl)
                    .WithHeader("Authorization", _uploadAuthorizationToken)
                    .WithHeader("X-Bz-File-Name", fileLocation)
                    .WithHeader("X-Bz-Content-Sha1", sha1)
                    .WithHeader("Content-Type", contentType);

                foreach (var (name, value) in info)
                    request = request.WithHeader($"X-Bz-Info-{name}", HttpUtility.UrlEncode(value));

                return await request
                    .PostContentReceiveJsonExplicitlyAsync<B2UploadResponse>(new StreamContent(stream));
            }
            catch (FlurlHttpException httpException)
            {
                isFirstAttempt = false;

                // We didn't even get a response back, rip
                if (httpException.Call.Response == null)
                    throw;

                var statusCode = httpException.Call.Response.StatusCode;
                if (statusCode != 401 && statusCode != 503)
                    throw;
            }
        }
    }

    private async Task RefreshAuthorizationAsync(bool force = false)
    {
        using var _ = await _mutex.LockAsync();

        if (!force && DateTimeOffset.UtcNow.Subtract(_authorizedWhen) < TokenExpirationInterval)
            return;

        var uploadInformation = await GetUploadInformationAsync();

        _authorizedWhen = DateTimeOffset.UtcNow;
        _uploadUrl = uploadInformation.UploadUrl;
        _uploadAuthorizationToken = uploadInformation.AuthorizationToken;
    }

    /// <summary>
    /// Retrieves information about <b>where</b> objects can be uploaded to.
    /// </summary>
    private async Task<B2UploadInformation> GetUploadInformationAsync()
    {
        return await _client.AuthorizedRequestAsync(request => request.PostJsonReceiveJsonExplicitlyAsync<B2UploadInformation>(new
        {
            BucketId = _bucket.Id
        }), B2Endpoints.GetUploadUrl);
    }
}

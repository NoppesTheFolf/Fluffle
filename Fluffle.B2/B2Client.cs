using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nito.AsyncEx;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Noppes.Fluffle.B2
{
    /// <summary>
    /// A bare minimum implementation of the Backblaze B2 API endpoints which are required by
    /// Fluffle. Because of this, authentication is only possible through application keys.
    /// </summary>
    public class B2Client : IDisposable
    {
        public Uri DownloadUri { get; private set; }

        /// <summary>
        /// The amount of time after which an authorization token will be considered due for a
        /// refresh. The token expired every 24 hours, so picking 23 hours is a safe bet.
        /// </summary>
        private static readonly TimeSpan TokenExpirationInterval = TimeSpan.FromHours(23);

        private string _authorizationToken;
        private DateTimeOffset _authorizedWhen;
        private B2Bucket _authorizedBucket;

        private readonly bool _hasCustomDownloadUri;
        private readonly string _credentials;
        private readonly IFlurlClient _httpClient;
        private readonly AsyncLock _mutex;

        public B2Client(string applicationKeyId, string applicationKey, string downloadUri = null)
        {
            _hasCustomDownloadUri = downloadUri != null;

            if (downloadUri != null)
                DownloadUri = new Uri(downloadUri, UriKind.Absolute);

            _mutex = new AsyncLock();
            _authorizedWhen = DateTimeOffset.MinValue;
            _credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(applicationKeyId + ":" + applicationKey));
            _httpClient = new FlurlClient().Configure(settings =>
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                };
                settings.JsonSerializer = new NewtonsoftJsonSerializer(jsonSettings);
            });
        }

        internal IFlurlRequest Request(params object[] urlSegments) => _httpClient.Request(urlSegments);

        /// <summary>
        /// Makes an authorized request to the Backblaze B2 API. Automatically retries the request
        /// and refreshes the authorization token if it has expired or has been invalidated.
        /// </summary>
        internal async Task<T> AuthorizedRequestAsync<T>(Func<IFlurlRequest, Task<T>> makeAndDoRequest, params object[] urlSegments)
        {
            var isFirstRequest = true;
            while (true)
            {
                try
                {
                    await RefreshAuthorizationAsync(!isFirstRequest);

                    var authorizedRequest = Request(urlSegments)
                        .WithHeader("Authorization", _authorizationToken);

                    return await makeAndDoRequest(authorizedRequest);
                }
                catch (FlurlHttpException httpException)
                {
                    if (httpException.Call.Response == null)
                        throw;

                    isFirstRequest = false;

                    var error = await httpException.Call.Response.GetJsonAsync<B2ErrorResponse>();
                    if (error.Code == B2ErrorCode.BadAuthorizationToken || error.Code == B2ErrorCode.ExpiredAuthorizationToken)
                        continue;

                    throw;
                }
            }
        }

        /// <summary>
        /// Refreshes the authorization token which is used to authorize requests with. Refreshing
        /// only occurs if the token is expired or if refreshing is forced.
        /// </summary>
        private async Task RefreshAuthorizationAsync(bool force = false)
        {
            using var _ = await _mutex.LockAsync();

            if (!force && DateTimeOffset.Now.Subtract(_authorizedWhen) < TokenExpirationInterval)
                return;

            var response = await Request(B2Endpoints.Authorize)
                .WithHeader("Authorization", $"Basic {_credentials}")
                .GetJsonAsync<B2AuthorizeResponse>();

            if (!_hasCustomDownloadUri)
                DownloadUri = new Uri(response.DownloadUrl, UriKind.Absolute);

            _httpClient.BaseUrl = response.ApiUrl;
            _authorizationToken = response.AuthorizationToken;
            _authorizedWhen = DateTimeOffset.Now;
            _authorizedBucket = new B2Bucket(response.Allowed.BucketId, response.Allowed.BucketName, this);
        }

        /// <summary>
        /// Gets the bucket to which the provided credentials grants permission.
        /// </summary>
        public async Task<B2Bucket> GetBucketAsync()
        {
            if (_authorizedBucket != null)
                return _authorizedBucket;

            await RefreshAuthorizationAsync();
            return _authorizedBucket;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}

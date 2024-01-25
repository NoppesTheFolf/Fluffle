using Flurl.Http;
using Noppes.Fluffle.Http;
using Serilog;
using System.Net;

namespace Noppes.Fluffle.Twitter.Client;

public interface ITwitterApiClient
{
    Task<TwitterUserModel> GetUserByIdAsync(string userId);

    Task<TwitterUserModel> GetUserByUsernameAsync(string username);

    IAsyncEnumerable<ICollection<TwitterTweetModel>> EnumerateUserMediaAsync(string userId);

    Task<TwitterGetMediaResponseModel> GetUserMediaAsync(string userId, string? cursor = null);

    Task<Stream> GetStreamAsync(string url, bool resilient);
}

public class TwitterApiClient : ApiClient, ITwitterApiClient
{
    private readonly string _apiKey;

    public TwitterApiClient(string baseUrl, string apiKey) : base(baseUrl)
    {
        _apiKey = apiKey;
    }

    public async Task<TwitterUserModel> GetUserByIdAsync(string userId) =>
        await GetUserAsync(Request("user/by-id", userId));

    public async Task<TwitterUserModel> GetUserByUsernameAsync(string username) =>
        await GetUserAsync(Request("user/by-screen-name", username));

    private async Task<TwitterUserModel> GetUserAsync(IFlurlRequest request)
    {
        try
        {
            var user = await ExecuteRequest<TwitterUserModel>(request);
            return user;
        }
        catch (FlurlHttpException e)
        {
            if (e.StatusCode != 404)
                throw;

            var error = await e.GetResponseJsonAsync<TwitterUserErrorModel>();
            throw new TwitterUserException(error);
        }
    }

    public async IAsyncEnumerable<ICollection<TwitterTweetModel>> EnumerateUserMediaAsync(string userId)
    {
        string? cursor = null;
        while (true)
        {
            var media = await GetUserMediaAsync(userId, cursor);
            if (!media.Tweets.Any())
                break;

            yield return media.Tweets;
            cursor = media.Next;
        }
    }

    public async Task<TwitterGetMediaResponseModel> GetUserMediaAsync(string userId, string? cursor = null)
    {
        var request = Request("user", userId, "media")
            .SetQueryParam("cursor", cursor);

        var media = await ExecuteRequest<TwitterGetMediaResponseModel>(request);
        return media;
    }

    private static async Task<T> ExecuteRequest<T>(IFlurlRequest request)
    {
        while (true)
        {
            try
            {
                var result = await request.GetJsonExplicitlyAsync<T>();

                return result;
            }
            catch (FlurlHttpException e)
            {
                if (e.StatusCode != (int)HttpStatusCode.TooManyRequests)
                    throw;

                if (e.Call.Response?.Headers == null || !e.Call.Response.Headers.TryGetFirst("rate-limit-reset", out var resetAtUnix))
                    throw;

                var resetAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(resetAtUnix));
                var resetsIn = resetAt.Subtract(DateTimeOffset.UtcNow);
                if (resetsIn > TimeSpan.Zero)
                {
                    Log.Information("Rate limit encountered. Retrying {url} in {time}", request.Url, resetsIn);
                    await Task.Delay(resetsIn);
                }
                else
                {
                    Log.Information("Retrying {url}", request.Url);
                }
            }
        }
    }

    private static readonly FlurlRetryPolicyBuilder DownloadRetryPolicy = new FlurlRetryPolicyBuilder()
        .WithStatusCode(HttpStatusCode.GatewayTimeout)
        .ShouldRetryClientTimeouts(true)
        .ShouldRetryNetworkErrors(true)
        .WithRetry(3, retryCount => TimeSpan.FromSeconds(5 * retryCount));

    public async Task<Stream> GetStreamAsync(string url, bool resilient)
    {
        var request = Request("download").SetQueryParam("url", url);
        Task<Stream> MakeRequest() => request.GetStreamAsync();

        var stream = resilient ? await DownloadRetryPolicy.Execute(MakeRequest) : await MakeRequest();
        return stream;
    }

    public override IFlurlRequest Request(params object[] urlSegments)
    {
        var request = base.Request(urlSegments)
            .WithHeader("Api-Key", _apiKey);

        return request;
    }
}

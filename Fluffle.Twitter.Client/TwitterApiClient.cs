using Flurl.Http;
using Noppes.Fluffle.Http;
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
        await MakeUserRequest(Request("user/by-id", userId));

    public async Task<TwitterUserModel> GetUserByUsernameAsync(string username) =>
        await MakeUserRequest(Request("user/by-screen-name", username));

    private static async Task<TwitterUserModel> MakeUserRequest(IFlurlRequest request)
    {
        try
        {
            var user = await request.GetJsonExplicitlyAsync<TwitterUserModel>();
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
        var response = await Request("user", userId, "media")
            .SetQueryParam("cursor", cursor)
            .GetJsonExplicitlyAsync<TwitterGetMediaResponseModel>();

        return response;
    }

    private static readonly FlurlRetryPolicyBuilder DownloadRetryPolicy = new FlurlRetryPolicyBuilder()
        .WithStatusCode(HttpStatusCode.GatewayTimeout)
        .ShouldRetryClientTimeouts(true)
        .ShouldRetryNetworkErrors(true)
        .WithRetry(3, retryCount => TimeSpan.FromSeconds(5 * retryCount));

    public async Task<Stream> GetStreamAsync(string url, bool resilient)
    {
        Task<Stream> MakeRequest() => FlurlClient.Request(url).GetStreamAsync();
        var stream = resilient ? await DownloadRetryPolicy.Execute(MakeRequest) : await MakeRequest();

        return stream;
    }

    public override IFlurlRequest Request(params object[] urlSegments)
    {
        return base.Request(urlSegments)
            .WithHeader("Api-Key", _apiKey);
    }
}

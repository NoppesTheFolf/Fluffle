using Flurl.Http;
using Noppes.Fluffle.Http;

namespace Noppes.Fluffle.Twitter.Client;

public interface ITwitterApiClient
{
    Task<TwitterUserModel> GetUserAsync(string username);

    IAsyncEnumerable<ICollection<TwitterTweetModel>> EnumerateUserMediaAsync(string userId);

    Task<TwitterGetMediaResponseModel> GetUserMediaAsync(string userId, string? cursor = null);

    Task<Stream> GetStreamAsync(string url);
}

public class TwitterApiClient : ApiClient, ITwitterApiClient
{
    private readonly string _apiKey;

    public TwitterApiClient(string baseUrl, string apiKey) : base(baseUrl)
    {
        _apiKey = apiKey;
    }

    public async Task<TwitterUserModel> GetUserAsync(string username)
    {
        try
        {
            var user = await Request("user", username)
                .GetJsonExplicitlyAsync<TwitterUserModel>();

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

    public async Task<Stream> GetStreamAsync(string url) => await FlurlClient.Request(url).GetStreamAsync();

    public override IFlurlRequest Request(params object[] urlSegments)
    {
        return base.Request(urlSegments)
            .WithHeader("Api-Key", _apiKey);
    }
}

namespace Fluffle.Feeder.Bluesky.JetstreamProcessor.ApiClient;

public interface IBlueskyApiClient
{
    Task<BlueskyApiProfile> GetProfileAsync(string did);

    Task<Stream> GetStreamAsync(string url);
}

namespace Fluffle.Feeder.Bluesky.JetstreamProcessor.ApiClient;

public class BlueskyApiException : Exception
{
    public BlueskyApiError Error { get; }

    public BlueskyApiException(BlueskyApiError error)
    {
        Error = error;
    }
}

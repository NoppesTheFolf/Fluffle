namespace Fluffle.Feeder.Bluesky.JetstreamProcessor.ApiClient;

public class BlueskyApiProfile
{
    public required string Did { get; set; }

    public required string Handle { get; set; }

    public string? DisplayName { get; set; }
}

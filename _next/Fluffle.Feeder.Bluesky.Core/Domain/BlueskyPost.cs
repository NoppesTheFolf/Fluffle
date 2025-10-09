namespace Fluffle.Feeder.Bluesky.Core.Domain;

public class BlueskyPost
{
    public required BlueskyPostId Id { get; set; }

    public required long UnixTimeMicroseconds { get; set; }

    public required IList<BlueskyImagePrediction> Images { get; set; }
}

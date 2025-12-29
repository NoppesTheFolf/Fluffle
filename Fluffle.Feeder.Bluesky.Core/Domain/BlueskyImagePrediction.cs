namespace Fluffle.Feeder.Bluesky.Core.Domain;

public class BlueskyImagePrediction
{
    public required string Link { get; set; }

    public required string MimeType { get; set; }

    public required float Prediction { get; set; }

    public required DateTime When { get; set; }
}

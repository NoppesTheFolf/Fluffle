namespace Fluffle.Feeder.Bluesky.Core.Domain;

public class BlueskyProfile
{
    public required string Did { get; set; }

    public required string? Handle { get; set; }

    public required string? DisplayName { get; set; }

    public required IList<BlueskyImagePrediction> ImagePredictions { get; set; }
}

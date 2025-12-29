namespace Fluffle.Inference.Api.Client;

public interface IInferenceApiClient
{
    Task<float[][]> ExactMatchV2Async(IList<Stream> imageStreams);

    Task<float> BlueskyFurryArtAsync(Stream imageStream);
}

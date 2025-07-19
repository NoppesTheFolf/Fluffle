namespace Fluffle.Inference.Api.Client;

public interface IInferenceApiClient
{
    Task<float[][]> ExactMatchV2Async(IList<Stream> imageStreams);
}

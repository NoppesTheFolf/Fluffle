namespace Fluffle.Inference.Api.Client;

public interface IInferenceApiClient
{
    Task<float[][]> ExactMatchV1Async(IList<Stream> imageStreams);

    Task<float[][]> ExactMatchV2Async(IList<Stream> imageStreams);
}

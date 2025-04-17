namespace Fluffle.Inference.Api.Client;

public interface IInferenceApiClient
{
    Task<float[][]> CreateAsync(IList<Stream> imageStreams);
}

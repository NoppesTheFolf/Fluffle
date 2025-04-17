using System.Net.Http.Json;

namespace Fluffle.Inference.Api.Client;

internal class InferenceApiClient : IInferenceApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public InferenceApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<float[][]> CreateAsync(IList<Stream> imageStreams)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(InferenceApiClient));

        using var content = new MultipartFormDataContent();
        foreach (var imageStream in imageStreams)
        {
            content.Add(new StreamContent(imageStream), "images", "image");
        }

        using var response = await httpClient.PostAsync("/", content);
        response.EnsureSuccessStatusCode();
        var vectors = await response.Content.ReadFromJsonAsync<float[][]>();

        return vectors!;
    }
}

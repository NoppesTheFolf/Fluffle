using System.Net.Http.Json;

namespace Fluffle.Inference.Api.Client;

internal class InferenceApiClient : IInferenceApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public InferenceApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<float[][]> ExactMatchV2Async(IList<Stream> imageStreams)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(InferenceApiClient));

        using var content = new MultipartFormDataContent();
        foreach (var imageStream in imageStreams)
        {
            content.Add(new StreamContent(imageStream), "images", "image");
        }

        using var response = await httpClient.PostAsync("/exact-match-v2", content);
        response.EnsureSuccessStatusCode();
        var vectors = await response.Content.ReadFromJsonAsync<float[][]>();

        return vectors!;
    }

    public async Task<float> BlueskyFurryArtAsync(Stream imageStream)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(InferenceApiClient));

        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(imageStream), "image", "image");

        using var response = await httpClient.PostAsync("/bluesky-furry-art", content);
        response.EnsureSuccessStatusCode();
        var prediction = await response.Content.ReadFromJsonAsync<float>();

        return prediction;
    }
}

using System.Net.Http.Json;

namespace Fluffle.Inference.Api.Client;

internal class InferenceApiClient : IInferenceApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public InferenceApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<float[][]> ExactMatchV1Async(IList<Stream> imageStreams)
    {
        return await RunInferenceAsync("exact-match-v1", imageStreams);
    }

    public async Task<float[][]> ExactMatchV2Async(IList<Stream> imageStreams)
    {
        return await RunInferenceAsync("exact-match-v2", imageStreams);
    }

    private async Task<float[][]> RunInferenceAsync(string path, IList<Stream> imageStreams)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(InferenceApiClient));

        using var content = new MultipartFormDataContent();
        foreach (var imageStream in imageStreams)
        {
            content.Add(new StreamContent(imageStream), "images", "image");
        }

        using var response = await httpClient.PostAsync($"/{path}", content);
        response.EnsureSuccessStatusCode();
        var vectors = await response.Content.ReadFromJsonAsync<float[][]>();

        return vectors!;
    }
}

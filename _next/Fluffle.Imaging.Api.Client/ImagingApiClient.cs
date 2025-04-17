using System.Net.Http.Json;

namespace Fluffle.Imaging.Api.Client;

internal class ImagingApiClient : IImagingApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ImagingApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ThumbnailModel> CreateThumbnailAsync(Stream imageStream, int size, int quality)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(ImagingApiClient));

        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(imageStream), "image", "image");

        using var response = await httpClient.PostAsync($"/thumbnail?size={size}&quality={quality}", content);
        response.EnsureSuccessStatusCode();

        var model = await response.Content.ReadFromJsonAsync<ThumbnailModel>();
        return model!;
    }
}

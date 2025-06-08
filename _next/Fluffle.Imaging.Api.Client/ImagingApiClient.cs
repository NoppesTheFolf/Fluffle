using Fluffle.Imaging.Api.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Fluffle.Imaging.Api.Client;

internal class ImagingApiClient : IImagingApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ImagingApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ImageMetadataModel> GetMetadataAsync(Stream imageStream)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(ImagingApiClient));

        using var content = new StreamContent(imageStream);
        using var response = await httpClient.PostAsync("/metadata", content);
        response.EnsureSuccessStatusCode();

        var model = await response.Content.ReadFromJsonAsync<ImageMetadataModel>();
        return model!;
    }

    public async Task<(byte[] thumbnail, ImageMetadataModel metadata)> CreateThumbnailAsync(Stream imageStream, int size, int quality, bool calculateCenter)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(ImagingApiClient));

        using var content = new StreamContent(imageStream);
        using var response = await httpClient.PostAsync($"/thumbnail?size={size}&quality={quality}&calculateCenter={calculateCenter}", content);
        response.EnsureSuccessStatusCode();

        var thumbnail = await response.Content.ReadAsByteArrayAsync();
        var metadataJson = response.Headers.GetValues("Imaging-Metadata").Single();
        var metadata = JsonSerializer.Deserialize<ImageMetadataModel>(metadataJson, JsonSerializerOptions.Web)!;

        return (thumbnail, metadata);
    }
}

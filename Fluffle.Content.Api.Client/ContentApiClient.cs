using System.Net;

namespace Fluffle.Content.Api.Client;

internal class ContentApiClient : IContentApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ContentApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task PutAsync(string path, Stream stream)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(ContentApiClient));

        using var content = new StreamContent(stream);
        using var response = await httpClient.PutAsync(path, content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Stream?> GetAsync(string path)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(ContentApiClient));

        using var response = await httpClient.GetAsync(path);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var buffer = await response.Content.ReadAsByteArrayAsync();
        return new MemoryStream(buffer);
    }

    public async Task DeleteAsync(string path)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(ContentApiClient));

        using var response = await httpClient.DeleteAsync(path);
        response.EnsureSuccessStatusCode();
    }
}

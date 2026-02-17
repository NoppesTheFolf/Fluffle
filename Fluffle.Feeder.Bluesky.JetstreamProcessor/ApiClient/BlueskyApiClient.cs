using System.Net.Http.Json;

namespace Fluffle.Feeder.Bluesky.JetstreamProcessor.ApiClient;

public class BlueskyApiClient : IBlueskyApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BlueskyApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<BlueskyApiProfile> GetProfileAsync(string did)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(BlueskyApiClient));

        var url = $"https://public.api.bsky.app/xrpc/app.bsky.actor.getProfile?actor={Uri.EscapeDataString(did)}";
        using var response = await httpClient.GetAsync(url);

        try
        {
            response.EnsureSuccessStatusCode();

            var apiProfile = await response.Content.ReadFromJsonAsync<BlueskyApiProfile>();
            return apiProfile!;
        }
        catch (HttpRequestException)
        {
            var apiError = await response.Content.ReadFromJsonAsync<BlueskyApiError>();
            throw new BlueskyApiException(apiError!);
        }
    }

    public async Task<Stream> GetStreamAsync(string url)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(BlueskyApiClient));
        var stream = await httpClient.GetStreamAsync(url);

        return stream;
    }
}

using System.Net.Http.Json;

namespace Fluffle.Feeder.Legacy.MainApi;

public class MainApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MainApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<CreditableEntitiesSyncModel> GetCreditableEntitiesAsync(string platform, long changeId)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(MainApiClient));

        var url = $"/api/v1/sync/creditable-entities/{Uri.EscapeDataString(platform)}/{changeId}";
        var model = await httpClient.GetFromJsonAsync<CreditableEntitiesSyncModel>(url);

        return model!;
    }

    public async Task<ImagesSyncModel> GetImagesAsync(string platform, long changeId)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(MainApiClient));

        var url = $"/api/v1/sync/images/{Uri.EscapeDataString(platform)}/{changeId}";
        var model = await httpClient.GetFromJsonAsync<ImagesSyncModel>(url);

        return model!;
    }
}
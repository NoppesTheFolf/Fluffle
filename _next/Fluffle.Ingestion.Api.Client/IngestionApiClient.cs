using Fluffle.Ingestion.Api.Models.ItemActions;
using System.Net;
using System.Net.Http.Json;

namespace Fluffle.Ingestion.Api.Client;

internal class IngestionApiClient : IIngestionApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IngestionApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task PutItemAsync(string itemId, PutItemModel item)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(IngestionApiClient));

        using var response = await httpClient.PutAsJsonAsync($"/api/items/{Uri.EscapeDataString(itemId)}", item);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteItemAsync(string itemId)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(IngestionApiClient));

        using var response = await httpClient.DeleteAsync($"/api/items/{Uri.EscapeDataString(itemId)}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<ItemActionModel?> DequeueItemActionAsync()
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(IngestionApiClient));

        using var response = await httpClient.GetAsync("/api/item-actions/dequeue");
        response.EnsureSuccessStatusCode();

        if (response.StatusCode == HttpStatusCode.NoContent)
            return null;

        var model = await response.Content.ReadFromJsonAsync<ItemActionModel>();
        return model!;
    }

    public async Task AcknowledgeItemActionAsync(string itemActionId)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(IngestionApiClient));

        using var response = await httpClient.PostAsync($"/api/item-actions/{Uri.EscapeDataString(itemActionId)}/acknowledge", null);
        response.EnsureSuccessStatusCode();
    }
}

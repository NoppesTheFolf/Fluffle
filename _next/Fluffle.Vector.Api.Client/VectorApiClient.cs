
using Fluffle.Vector.Api.Models.Items;
using Fluffle.Vector.Api.Models.Vectors;
using System.Net;
using System.Net.Http.Json;

namespace Fluffle.Vector.Api.Client;

internal class VectorApiClient : IVectorApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public VectorApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task PutItemAsync(string itemId, PutItemModel item)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(VectorApiClient));

        using var response = await httpClient.PutAsJsonAsync($"/items/{Uri.EscapeDataString(itemId)}", item);
        await response.EnsureSuccessAsync();
    }

    public async Task<ItemModel?> GetItemAsync(string itemId)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(VectorApiClient));

        using var response = await httpClient.GetAsync($"/items/{Uri.EscapeDataString(itemId)}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        await response.EnsureSuccessAsync();
        var model = await response.Content.ReadFromJsonAsync<ItemModel>();

        return model!;
    }

    public async Task<ICollection<string>> GetItemCollectionsAsync(string itemId)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(VectorApiClient));

        using var response = await httpClient.GetAsync($"/items/{Uri.EscapeDataString(itemId)}/collections");
        await response.EnsureSuccessAsync();

        var collections = await response.Content.ReadFromJsonAsync<ICollection<string>>();
        return collections!;
    }

    public async Task<ICollection<ItemModel>> GetItemsAsync(ICollection<string> itemIds)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(VectorApiClient));

        var parameters = string.Join("&", itemIds.Select(x => $"itemIds={Uri.EscapeDataString(x)}"));
        var models = await httpClient.GetFromJsonAsync<ICollection<ItemModel>>($"/items?{parameters}");

        return models!;
    }

    public async Task DeleteItemAsync(string itemId)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(VectorApiClient));

        using var response = await httpClient.DeleteAsync($"/items/{Uri.EscapeDataString(itemId)}");
        await response.EnsureSuccessAsync();
    }

    public async Task PutItemVectorsAsync(string itemId, string collectionId, ICollection<PutItemVectorModel> vectors)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(VectorApiClient));

        var url = $"/items/{Uri.EscapeDataString(itemId)}/vectors/{Uri.EscapeDataString(collectionId)}";
        using var response = await httpClient.PutAsJsonAsync(url, vectors);
        await response.EnsureSuccessAsync();
    }

    public async Task<IList<VectorSearchResultModel>> SearchCollectionAsync(string collectionId, VectorSearchParametersModel parameters)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(VectorApiClient));

        using var response = await httpClient.PostAsJsonAsync($"/collections/{Uri.EscapeDataString(collectionId)}/search", parameters);
        await response.EnsureSuccessAsync();

        var results = await response.Content.ReadFromJsonAsync<IList<VectorSearchResultModel>>();
        return results!;
    }
}

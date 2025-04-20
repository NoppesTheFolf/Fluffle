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
        response.EnsureSuccessStatusCode();
    }

    public async Task<ItemModel?> GetItemAsync(string itemId)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(VectorApiClient));

        using var response = await httpClient.GetAsync($"/items/{Uri.EscapeDataString(itemId)}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        var model = await response.Content.ReadFromJsonAsync<ItemModel>();

        return model!;
    }

    public async Task DeleteItemAsync(string itemId)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(VectorApiClient));

        using var response = await httpClient.DeleteAsync($"/items/{Uri.EscapeDataString(itemId)}");
        response.EnsureSuccessStatusCode();
    }

    public async Task PutItemVectorsAsync(string itemId, string modelId, ICollection<PutItemVectorModel> vectors)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(VectorApiClient));

        var url = $"/items/{Uri.EscapeDataString(itemId)}/vectors/{Uri.EscapeDataString(modelId)}";
        using var response = await httpClient.PutAsJsonAsync(url, vectors);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IList<VectorSearchResultModel>> SearchVectorsAsync(VectorSearchParametersModel parameters)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(VectorApiClient));

        using var response = await httpClient.PostAsJsonAsync("/vectors/search", parameters);
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<IList<VectorSearchResultModel>>();
        return results!;
    }
}

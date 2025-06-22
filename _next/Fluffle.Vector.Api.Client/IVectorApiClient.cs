using Fluffle.Vector.Api.Models.Items;
using Fluffle.Vector.Api.Models.Vectors;

namespace Fluffle.Vector.Api.Client;

public interface IVectorApiClient
{
    Task PutItemAsync(string itemId, PutItemModel item);

    Task<ItemModel?> GetItemAsync(string itemId);

    Task<ICollection<string>> GetItemCollectionsAsync(string itemId);

    Task<ICollection<ItemModel>> GetItemsAsync(ICollection<string> itemIds);

    Task DeleteItemAsync(string itemId);

    Task PutItemVectorsAsync(string itemId, string collectionId, ICollection<PutItemVectorModel> vectors);

    Task<IList<VectorSearchResultModel>> SearchCollectionAsync(string collectionId, VectorSearchParametersModel parameters);
}

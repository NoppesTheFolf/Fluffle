using Fluffle.Vector.Api.Models.Items;
using Fluffle.Vector.Api.Models.Vectors;

namespace Fluffle.Vector.Api.Client;

public interface IVectorApiClient
{
    Task PutItemAsync(string itemId, PutItemModel item);

    Task<ItemModel?> GetItemAsync(string itemId);

    Task DeleteItemAsync(string itemId);

    Task PutItemVectorsAsync(string itemId, string modelId, ICollection<PutItemVectorModel> vectors);

    Task<IList<VectorSearchResultModel>> SearchVectorsAsync(VectorSearchParametersModel parameters);
}

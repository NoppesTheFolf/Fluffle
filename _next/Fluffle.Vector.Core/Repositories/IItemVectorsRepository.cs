using Fluffle.Vector.Core.Domain.Items;
using Fluffle.Vector.Core.Domain.Vectors;

namespace Fluffle.Vector.Core.Repositories;

public interface IItemVectorsRepository
{
    Task UpsertAsync(Model model, Item item, ICollection<ItemVector> vectors);

    Task<ICollection<string>> GetCollectionsAsync(string itemId);

    Task<IList<VectorSearchResult>> GetAsync(string collectionId, float[] query, int limit);

    Task DeleteAsync(string itemId);
}

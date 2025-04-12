using Fluffle.Vector.Core.Domain.Items;

namespace Fluffle.Vector.Core.Repositories;

public interface IItemVectorsRepository
{
    Task UpsertAsync(ItemVectors itemVectors);

    Task ForEachAsync(string modelId, Action<ItemVectors> action, CancellationToken cancellationToken = default);
}

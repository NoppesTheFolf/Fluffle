using Fluffle.Vector.Core.Domain.Items;

namespace Fluffle.Vector.Core.Repositories;

public interface IItemRepository
{
    Task UpsertAsync(Item item);

    Task<Item?> GetAsync(string itemId);

    Task DeleteAsync(string itemId);
}

using Fluffle.Vector.Core.Domain.Items;

namespace Fluffle.Vector.Core.Repositories;

public interface IItemRepository
{
    Task UpsertAsync(Item item);

    Task<Item?> GetAsync(string itemId);

    Task<ICollection<Item>> GetAsync(ICollection<string>? itemIds, string? groupId);

    Task DeleteAsync(string itemId);
}

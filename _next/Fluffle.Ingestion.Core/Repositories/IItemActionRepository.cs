using Fluffle.Ingestion.Core.Domain.ItemActions;

namespace Fluffle.Ingestion.Core.Repositories;

public interface IItemActionRepository
{
    Task CreateAsync(ItemAction itemAction);

    Task<ItemAction?> GetByItemIdAsync(string itemId);

    Task<ItemAction?> GetHighestPriorityAsync();

    Task SetVisibleWhenAsync(string itemActionId, DateTime visibleWhen);

    Task DeleteAsync(string itemActionId);
}

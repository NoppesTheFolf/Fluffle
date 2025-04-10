using Fluffle.Ingestion.Core.Domain.ItemActions;
using Fluffle.Ingestion.Core.Domain.Items;
using Fluffle.Ingestion.Core.Repositories;
using Nito.AsyncEx;

namespace Fluffle.Ingestion.Core.Services;

public class ItemActionService
{
    private static readonly TimeSpan ProcessingTimeout = TimeSpan.FromMinutes(15);
    private static readonly AsyncLock Locker = new();

    private readonly IItemActionRepository _repository;

    public ItemActionService(IItemActionRepository repository)
    {
        _repository = repository;
    }

    public async Task EnqueueIndexAsync(Item item, int priority)
    {
        using var _ = await Locker.LockAsync();

        await EnqueueAsync(item.ItemId, visibleWhen => new IndexItemAction
        {
            ItemId = item.ItemId,
            Priority = priority,
            VisibleWhen = visibleWhen,
            Item = item
        });
    }

    public async Task EnqueueDeleteAsync(string itemId)
    {
        using var _ = await Locker.LockAsync();

        await EnqueueAsync(itemId, visibleWhen => new DeleteItemAction
        {
            ItemId = itemId,
            Priority = int.MaxValue,
            VisibleWhen = visibleWhen
        });
    }

    private async Task EnqueueAsync(string itemId, Func<DateTime, ItemAction> createItemAction)
    {
        var existingItemAction = await _repository.GetByItemIdAsync(itemId);
        var needsDelay = false;

        if (existingItemAction != null)
        {
            await _repository.DeleteAsync(existingItemAction.ItemActionId!);
            needsDelay = true;
        }

        var visibleWhen = DateTime.UtcNow;
        if (needsDelay)
            visibleWhen = visibleWhen.Add(ProcessingTimeout);

        var itemAction = createItemAction(visibleWhen);
        await _repository.CreateAsync(itemAction);
    }

    public async Task<ItemAction?> DequeueAsync()
    {
        using var _ = await Locker.LockAsync();

        var itemAction = await _repository.GetHighestPriorityAsync();
        if (itemAction == null)
            return null;

        await _repository.SetVisibleWhenAsync(itemAction.ItemActionId!, DateTime.UtcNow.Add(ProcessingTimeout));

        return itemAction;
    }

    public async Task CompleteAsync(string itemActionId)
    {
        using var _ = await Locker.LockAsync();

        await _repository.DeleteAsync(itemActionId);
    }
}

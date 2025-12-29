using Fluffle.Ingestion.Core.Domain.ItemActions;
using Fluffle.Ingestion.Core.Domain.Items;
using Fluffle.Ingestion.Core.Repositories;
using Nito.AsyncEx;

namespace Fluffle.Ingestion.Core.Services;

public class ItemActionService
{
    private static readonly AsyncLock Locker = new();

    private readonly IItemActionRepository _repository;
    private readonly IItemActionFailureRepository _failureRepository;

    public ItemActionService(IItemActionRepository repository, IItemActionFailureRepository failureRepository)
    {
        _repository = repository;
        _failureRepository = failureRepository;
    }

    public async Task<string> EnqueueIndexAsync(Item item, long priority)
    {
        using var _ = await Locker.LockAsync();

        var itemActionIds = new List<string>();

        var itemAction = await _repository.GetByItemIdAsync(item.ItemId);
        if (itemAction != null)
        {
            itemActionIds.Add(itemAction.ItemActionId!);
        }

        if (item.GroupId != null)
        {
            var itemActionsByGroup = await _repository.GetByGroupIdAsync(item.GroupId!);
            var deleteItemActionsByGroup = itemActionsByGroup
                .Where(x => x is DeleteGroupItemAction)
                .Select(x => x.ItemActionId!);

            itemActionIds.AddRange(deleteItemActionsByGroup);
        }

        return await EnqueueAsync(itemActionIds, visibleWhen => new IndexItemAction
        {
            ItemId = item.ItemId,
            GroupId = item.GroupId,
            Priority = priority,
            AttemptCount = 0,
            VisibleWhen = visibleWhen,
            Item = item
        });
    }

    public async Task<string> EnqueueDeleteItemAsync(string itemId)
    {
        using var _ = await Locker.LockAsync();

        var itemAction = await _repository.GetByItemIdAsync(itemId);
        ICollection<string> itemActionIds = itemAction == null ? [] : [itemAction.ItemActionId!];

        return await EnqueueAsync(itemActionIds, visibleWhen => new DeleteItemAction
        {
            ItemId = itemId,
            Priority = int.MaxValue,
            AttemptCount = 0,
            VisibleWhen = visibleWhen
        });
    }

    public async Task<string> EnqueueDeleteGroupAsync(string groupId)
    {
        using var _ = await Locker.LockAsync();

        var itemActions = await _repository.GetByGroupIdAsync(groupId);
        var itemActionIds = itemActions.Select(x => x.ItemActionId!).ToList();

        return await EnqueueAsync(itemActionIds, visibleWhen => new DeleteGroupItemAction
        {
            GroupId = groupId,
            Priority = int.MaxValue,
            AttemptCount = 0,
            VisibleWhen = visibleWhen
        });
    }

    private async Task<string> EnqueueAsync(ICollection<string> existingItemActions, Func<DateTime, ItemAction> createItemAction)
    {
        var needsDelay = false;
        if (existingItemActions.Count > 0)
        {
            await _repository.DeleteAsync(existingItemActions);
            needsDelay = true;
        }

        var visibleWhen = DateTime.UtcNow;
        if (needsDelay)
        {
            // If a worker has already picked up the existing item action, give it some time to finish before
            // the newly added action becomes available for processing
            visibleWhen = visibleWhen.Add(TimeSpan.FromMinutes(15));
        }

        var itemAction = createItemAction(visibleWhen);
        await _repository.CreateAsync(itemAction);

        return itemAction.ItemActionId!;
    }

    public async Task<ItemAction?> DequeueAsync()
    {
        using var _ = await Locker.LockAsync();

        ItemAction? itemAction;
        do
        {
            itemAction = await _repository.GetHighestPriorityAsync();
            if (itemAction == null)
                return null;

            // Give up after 10 attempts
            if (itemAction.AttemptCount < 10)
                break;

            await _failureRepository.CreateAsync(itemAction);
            await _repository.DeleteAsync(itemAction.ItemActionId!);
            itemAction = null;
        } while (itemAction == null);

        var processingTimeout = GetProcessingTimeout(itemAction.AttemptCount);
        var visibleWhen = DateTime.UtcNow.Add(processingTimeout);
        await _repository.SetVisibleWhenAsync(itemAction.ItemActionId!, visibleWhen);
        await _repository.IncrementAttemptCountAsync(itemAction.ItemActionId!);

        return itemAction;
    }

    public async Task AcknowledgeAsync(string itemActionId)
    {
        using var _ = await Locker.LockAsync();

        await _repository.DeleteAsync(itemActionId);
    }

    private static TimeSpan GetProcessingTimeout(int attemptCount)
    {
        // Limit the timeout to a day after having already tried 5 times
        if (attemptCount >= 5)
            return TimeSpan.FromDays(1);

        return TimeSpan.FromMinutes(15 * Math.Pow(2, attemptCount));
    }
}

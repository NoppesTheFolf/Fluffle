﻿using Fluffle.Ingestion.Core.Domain.ItemActions;
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

    public async Task EnqueueIndexAsync(Item item, long priority)
    {
        using var _ = await Locker.LockAsync();

        await EnqueueAsync(item.ItemId, visibleWhen => new IndexItemAction
        {
            ItemId = item.ItemId,
            Priority = priority,
            AttemptCount = 0,
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
            AttemptCount = 0,
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
        {
            // If a worker has already picked up the existing item action, give it some time to finish before
            // the newly added action becomes available for processing
            visibleWhen = visibleWhen.Add(TimeSpan.FromMinutes(15));
        }

        var itemAction = createItemAction(visibleWhen);
        await _repository.CreateAsync(itemAction);
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

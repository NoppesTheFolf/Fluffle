namespace Noppes.Fluffle.Queue;

public interface IQueue<T>
{
    /// <summary>
    /// Enqueue an item.
    /// </summary>
    public Task EnqueueAsync(T? value, TimeSpan? visibleAfter, TimeSpan? expireAfter);

    /// <summary>
    /// Enqueue items.
    /// </summary>
    public Task EnqueueManyAsync(IEnumerable<T?> values, TimeSpan? visibleAfter, TimeSpan? expireAfter);

    /// <summary>
    /// Dequeue an item.
    /// </summary>
    public Task<QueueItem<T?>?> DequeueAsync();

    /// <summary>
    /// Dequeue a batch of items at once. If <paramref name="limit"/> is null, then allow the
    /// implementation to decide how many items to dequeue.
    /// </summary>
    public Task<ICollection<QueueItem<T?>>> DequeueManyAsync(int? limit = null);
}
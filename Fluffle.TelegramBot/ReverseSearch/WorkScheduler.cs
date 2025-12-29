using System;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Fluffle.TelegramBot.ReverseSearch;

public abstract class WorkSchedulerItem<TResult>
{
    public AsyncManualResetEvent CompletionEvent { get; set; }

    public TResult Result { get; set; }

    public Exception Exception { get; set; }
}

public abstract class WorkScheduler<T, TPriority, TResult> where T : WorkSchedulerItem<TResult>, new()
{
    private readonly int _numberOfWorkers;
    private readonly PriorityChannel<T, TPriority> _channel;
    private readonly AsyncLock _readLock;
    private DateTime? _lastReadAt;

    protected WorkScheduler(int numberOfWorkers)
    {
        _numberOfWorkers = numberOfWorkers;
        _channel = new PriorityChannel<T, TPriority>();
        _readLock = new AsyncLock();

        for (var i = 0; i < numberOfWorkers; i++)
            Task.Run(WorkAsync);
    }

    private async Task WorkAsync()
    {
        while (true)
        {
            // Wait for work to be available
            T item;
            using (var _ = await _readLock.LockAsync())
            {
                var interval = GetInterval();
                if (interval != null && _numberOfWorkers > 1)
                    throw new InvalidOperationException("Interval only works with a single worker.");

                if (interval == null)
                {
                    item = await _channel.ReadAsync();
                }
                else
                {
                    if (_lastReadAt != null)
                    {
                        var waitUntil = ((DateTime)_lastReadAt).AddMilliseconds((int)interval);
                        var timeToWait = waitUntil - DateTime.UtcNow;
                        if (timeToWait > TimeSpan.Zero)
                        {
                            await Task.Delay(timeToWait);
                        }
                    }

                    item = await _channel.ReadAsync();
                    _lastReadAt = DateTime.UtcNow;
                }
            }

            try
            {
                // Handle the work the implementation decides on
                item.Result = await HandleAsync(item);
            }
            catch (Exception e)
            {
                item.Exception = e;
            }
            finally
            {
                // Signal the work has been completed
                item.CompletionEvent.Set();
            }
        }
    }

    protected virtual int? GetInterval() => null;

    protected abstract Task<TResult> HandleAsync(T item);

    public virtual async Task<TResult> ProcessAsync(T item, TPriority priority)
    {
        // Schedule the work to be picked up by a worker
        item.CompletionEvent = new AsyncManualResetEvent();
        await _channel.WriteAsync(item, priority);

        // Wait for a worker to signal the work has been completed
        await item.CompletionEvent.WaitAsync();

        // Raise an exception if the worker caught one
        if (item.Exception != null)
            throw item.Exception;

        return item.Result;
    }
}

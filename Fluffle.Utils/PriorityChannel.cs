using Nito.AsyncEx;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Utils
{
    public class PriorityChannel<T, TPriority>
    {
        private readonly PriorityQueue<T, TPriority> _queue = new();
        private readonly AsyncLock _lock = new();
        private readonly SemaphoreSlim _semaphore = new(0);

        public async Task WriteAsync(T value, TPriority priority, CancellationToken cancellationToken = default)
        {
            using (var _ = await _lock.LockAsync(cancellationToken))
                _queue.Enqueue(value, priority);

            _semaphore.Release();
        }

        public async ValueTask<T> ReadAsync(CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            using var _ = await _lock.LockAsync(cancellationToken);
            var item = _queue.Dequeue();

            return item;
        }
    }
}

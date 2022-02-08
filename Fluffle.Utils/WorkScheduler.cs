using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Utils
{
    public abstract class WorkSchedulerItem<TResult>
    {
        public AsyncManualResetEvent CompletionEvent { get; set; }

        public TResult Result { get; set; }

        public Exception Exception { get; set; }
    }

    public abstract class WorkScheduler<T, TPriority, TResult> where T : WorkSchedulerItem<TResult>, new()
    {
        private readonly PriorityChannel<T, TPriority> _channel;

        protected WorkScheduler(int numberOfWorkers)
        {
            _channel = new PriorityChannel<T, TPriority>();

            for (var i = 0; i < numberOfWorkers; i++)
                Task.Run(WorkAsync);
        }

        private async Task WorkAsync()
        {
            while (true)
            {
                // Wait for work to be available
                var item = await _channel.ReadAsync();

                try
                {
                    // Handle the work the implementation decides on
                    item.Result = await HandleAsync(item);

                    // Signal the work has been completed
                    item.CompletionEvent.Set();
                }
                catch (Exception e)
                {
                    item.Exception = e;
                }
            }
        }

        protected abstract Task<TResult> HandleAsync(T item);

        public async Task<TResult> ProcessAsync(T item, TPriority priority)
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
}

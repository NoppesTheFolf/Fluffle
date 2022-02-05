using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Bot.Utils
{
    public class TaskAwaiter<T>
    {
        private readonly HashSet<Task> _tasks;
        private readonly ILogger<T> _logger;
        private readonly CancellationToken _cancellationToken;
        private readonly Task _continuouslyRemoveCompletedTask;

        public TaskAwaiter(ILogger<T> logger, CancellationToken cancellationToken)
        {
            _logger = logger;
            _cancellationToken = cancellationToken;
            _tasks = new HashSet<Task>();
            _continuouslyRemoveCompletedTask = Task.Run(ContinuouslyRemoveCompleted);
        }

        public void Add(Task task)
        {
            if (_cancellationToken.IsCancellationRequested)
                throw new InvalidOperationException();

            lock (_tasks)
            {
                _tasks.Add(task);
            }
        }

        private async Task ContinuouslyRemoveCompleted()
        {
            while (true)
            {
                if (_cancellationToken.IsCancellationRequested)
                    return;

                lock (_tasks)
                {
                    var removedCount = _tasks.RemoveWhere(x => x.IsCompleted);
                    if (removedCount > 0)
                        _logger.LogDebug("Removed {count} tasks.", removedCount);
                }

                await Task.Delay(500);
            }
        }

        public async Task WaitTillAllCompleted()
        {
            await _continuouslyRemoveCompletedTask;

            while (true)
            {
                lock (_tasks)
                {
                    var areAllCompleted = _tasks.All(x => x.IsCompleted);

                    if (areAllCompleted)
                        break;

                    _logger.LogDebug("Waiting for remaining tasks to complete...");
                }

                await Task.Delay(200);
            }
        }
    }
}

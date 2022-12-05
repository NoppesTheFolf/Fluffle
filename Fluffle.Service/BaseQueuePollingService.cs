using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Queue;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Service;

public abstract class BaseQueuePollingService<TService, TQueueEntity> : ScheduledService<TService> where TService : Service
{
    protected abstract TimeSpan VisibleAfter { get; }

    private readonly IQueue<TQueueEntity> _queue;
    private readonly ILogger<TService> _logger;

    protected BaseQueuePollingService(IServiceProvider services) : base(services)
    {
        _queue = Services.GetRequiredService<IQueue<TQueueEntity>>();
        _logger = Services.GetRequiredService<ILogger<TService>>();
    }

    protected override async Task RunAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            var items = await _queue.DequeueManyAsync(VisibleAfter);
            if (items.Count == 0)
            {
                _logger.LogDebug("No more items left in queue.");
                break;
            }

            await ProcessQueueItems(items, stoppingToken);
        }
    }

    public abstract Task ProcessQueueItems(ICollection<QueueItem<TQueueEntity>> items, CancellationToken stoppingToken);
}
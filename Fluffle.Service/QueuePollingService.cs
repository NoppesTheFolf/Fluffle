using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Queue;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Service;

public abstract class QueuePollingService<TService, TQueueEntity> : BaseQueuePollingService<TService, TQueueEntity> where TService : Service
{
    private readonly ILogger<TService> _logger;

    protected QueuePollingService(IServiceProvider services) : base(services)
    {
        _logger = services.GetRequiredService<ILogger<TService>>();
    }

    public override async Task ProcessQueueItems(ICollection<QueueItem<TQueueEntity>> items, CancellationToken stoppingToken)
    {
        foreach (var item in items)
        {
            _logger.LogDebug("Processing queue item.");
            await ProcessAsync(item.Value, stoppingToken);

            _logger.LogDebug("Acknowledge queue item has been processed.");
            await item.AcknowledgeAsync();
        }
    }

    public abstract Task ProcessAsync(TQueueEntity value, CancellationToken cancellationToken);
}
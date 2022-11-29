using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Service;

public abstract class QueuePollingBatchService<TService, TQueueEntity> : BaseQueuePollingService<TService, TQueueEntity> where TService : Service
{
    private readonly ILogger<TService> _logger;

    protected QueuePollingBatchService(IServiceProvider services) : base(services)
    {
        _logger = services.GetRequiredService<ILogger<TService>>();
    }

    public override async Task ProcessQueueItems(ICollection<QueueItem<TQueueEntity>> items, CancellationToken stoppingToken)
    {
        var values = items.Select(x => x.Value).ToList();

        _logger.LogDebug("Processing queue items.");
        await ProcessAsync(values, stoppingToken);

        _logger.LogDebug("Acknowledging queue items have been processed.");
        foreach (var item in items)
        {
            await item.AcknowledgeAsync();
        }
    }

    public abstract Task ProcessAsync(ICollection<TQueueEntity> values, CancellationToken cancellationToken);
}

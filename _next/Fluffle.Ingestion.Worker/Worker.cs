using Fluffle.Ingestion.Api.Client;
using Fluffle.Ingestion.Worker.ItemActionHandlers;

namespace Fluffle.Ingestion.Worker;

public class Worker : BackgroundService
{
    private readonly IIngestionApiClient _ingestionApiClient;
    private readonly ItemActionHandlerFactory _itemActionHandlerFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(IIngestionApiClient ingestionApiClient, ItemActionHandlerFactory itemActionHandlerFactory, ILogger<Worker> logger)
    {
        _ingestionApiClient = ingestionApiClient;
        _itemActionHandlerFactory = itemActionHandlerFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var itemAction = await _ingestionApiClient.DequeueItemActionAsync();
            if (itemAction != null)
            {
                using (_logger.BeginScope("ItemActionId:{ItemActionId} ItemId:{ItemId}", itemAction.ItemActionId, itemAction.ItemId))
                {
                    var handler = itemAction.Visit(_itemActionHandlerFactory);

                    try
                    {
                        await handler.RunAsync();

                        await _ingestionApiClient.AcknowledgeItemActionAsync(itemAction.ItemActionId);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "An exception occurred while processing an item action.");
                    }
                }
            }
            else
            {
                var interval = TimeSpan.FromSeconds(15);
                _logger.LogInformation("No item to process, trying again in {Interval}.", interval);
                await Task.Delay(interval, stoppingToken);
            }
        }
    }
}

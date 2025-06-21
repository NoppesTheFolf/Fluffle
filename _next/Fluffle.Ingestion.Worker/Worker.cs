using Fluffle.Ingestion.Api.Client;
using Fluffle.Ingestion.Worker.ItemActionHandlers;
using Fluffle.Ingestion.Worker.Telemetry;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace Fluffle.Ingestion.Worker;

public class Worker : BackgroundService
{
    private readonly IIngestionApiClient _ingestionApiClient;
    private readonly ItemActionHandlerFactory _itemActionHandlerFactory;
    private readonly IOptions<WorkerOptions> _options;
    private readonly ILogger<Worker> _logger;
    private readonly TelemetryClient _telemetryClient;

    public Worker(
        IIngestionApiClient ingestionApiClient,
        ItemActionHandlerFactory itemActionHandlerFactory,
        IOptions<WorkerOptions> options,
        ILogger<Worker> logger,
        TelemetryClient telemetryClient)
    {
        _ingestionApiClient = ingestionApiClient;
        _itemActionHandlerFactory = itemActionHandlerFactory;
        _options = options;
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workerId = RuntimeHelpers.GetHashCode(this);
        using var _ = _logger.BeginScope("WorkerId:{WorkerId}", workerId);
        _logger.LogInformation("Started handler worker.");

        try
        {
            await ExecuteAsyncInternal(stoppingToken);
        }
        finally
        {
            _logger.LogInformation("Stopped handler worker.");
        }
    }

    private async Task ExecuteAsyncInternal(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var itemAction = await _ingestionApiClient.DequeueItemActionAsync().Timed(_telemetryClient, "IngestionApiDequeueItemAction");
            if (itemAction != null)
            {
                using (_logger.BeginScope("ItemActionId:{ItemActionId} ItemId:{ItemId}", itemAction.ItemActionId, itemAction.ItemId))
                {
                    var handler = itemAction.Visit(_itemActionHandlerFactory);

                    try
                    {
                        await handler.RunAsync();

                        await _ingestionApiClient.AcknowledgeItemActionAsync(itemAction.ItemActionId).Timed(_telemetryClient, "IngestionApiAcknowledgeItemAction");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "An exception occurred while processing an item action.");
                        await Task.Delay(_options.Value.ErrorDelay, stoppingToken);
                    }
                }
            }
            else
            {
                _logger.LogInformation("No item to process, trying again in {Interval}.", _options.Value.DequeueInterval);
                await Task.Delay(_options.Value.DequeueInterval, stoppingToken);
            }
        }
    }
}

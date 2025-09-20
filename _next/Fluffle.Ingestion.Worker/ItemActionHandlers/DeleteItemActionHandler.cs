using Fluffle.Ingestion.Api.Models.ItemActions;
using Fluffle.Ingestion.Worker.Telemetry;
using Fluffle.Ingestion.Worker.ThumbnailStorage;
using Fluffle.Vector.Api.Client;
using Microsoft.ApplicationInsights;

namespace Fluffle.Ingestion.Worker.ItemActionHandlers;

public class DeleteItemActionHandler : IItemActionHandler
{
    private readonly DeleteItemActionModel _itemAction;
    private readonly IVectorApiClient _vectorApiClient;
    private readonly IThumbnailStorage _thumbnailStorage;
    private readonly ILogger<DeleteItemActionHandler> _logger;
    private readonly TelemetryClient _telemetryClient;

    public DeleteItemActionHandler(DeleteItemActionModel itemAction, IServiceProvider serviceProvider)
    {
        _itemAction = itemAction;
        _vectorApiClient = serviceProvider.GetRequiredService<IVectorApiClient>();
        _thumbnailStorage = serviceProvider.GetRequiredService<IThumbnailStorage>();
        _logger = serviceProvider.GetRequiredService<ILogger<DeleteItemActionHandler>>();
        _telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();
    }

    public async Task RunAsync()
    {
        var itemId = _itemAction.ItemId;
        using (_logger.BeginScope("ItemId:{ItemId}", itemId))
        {
            _logger.LogInformation("Deleting item...");

            var item = await _vectorApiClient.GetItemAsync(itemId).Timed(_telemetryClient, "VectorApiGetItem");
            if (item == null)
            {
                _logger.LogInformation("No item was found on the Vector API to delete, skipping.");
                return;
            }

            _logger.LogInformation("Deleting item from thumbnail storage.");
            await _thumbnailStorage.DeleteAsync(itemId).Timed(_telemetryClient, "ContentApiDeleteThumbnail");

            _logger.LogInformation("Deleting item from Vector API.");
            await _vectorApiClient.DeleteItemAsync(itemId).Timed(_telemetryClient, "VectorApiDeleteItem");

            _logger.LogInformation("Item deleted!");
            _telemetryClient.GetMetric("ItemsDeleted", "Platform").TrackValue(1, _itemAction.ItemId.Split('_', 2)[0]);
        }
    }
}

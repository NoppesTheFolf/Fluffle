using Fluffle.Ingestion.Api.Client;
using Fluffle.Ingestion.Api.Models.ItemActions;
using Fluffle.Ingestion.Worker.Telemetry;
using Fluffle.Vector.Api.Client;
using Microsoft.ApplicationInsights;

namespace Fluffle.Ingestion.Worker.ItemActionHandlers;

public class DeleteGroupItemActionHandler : IItemActionHandler
{
    private readonly DeleteGroupItemActionModel _itemAction;
    private readonly IVectorApiClient _vectorApiClient;
    private readonly IIngestionApiClient _ingestionApiClient;
    private readonly ILogger<DeleteGroupItemActionHandler> _logger;
    private readonly TelemetryClient _telemetryClient;

    public DeleteGroupItemActionHandler(DeleteGroupItemActionModel itemAction, IServiceProvider serviceProvider)
    {
        _itemAction = itemAction;
        _vectorApiClient = serviceProvider.GetRequiredService<IVectorApiClient>();
        _ingestionApiClient = serviceProvider.GetRequiredService<IIngestionApiClient>();
        _logger = serviceProvider.GetRequiredService<ILogger<DeleteGroupItemActionHandler>>();
        _telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();
    }

    public async Task RunAsync()
    {
        var groupId = _itemAction.GroupId;
        using (_logger.BeginScope("GroupId:{GroupId}", groupId))
        {
            _logger.LogInformation("Deleting group...");

            var items = await _vectorApiClient.GetItemsAsync(
                itemIds: null,
                groupId: groupId
            ).Timed(_telemetryClient, "VectorApiGetItems");

            if (items.Count == 0)
            {
                _logger.LogInformation("No items need to be deleted from group.");
                return;
            }

            var itemIds = items.Select(x => x.ItemId).ToList();
            _logger.LogInformation("Scheduling {ItemIds} to be deleted.", itemIds);
            var deleteItemActions = itemIds.Select(PutItemActionModel (x) => new PutDeleteItemActionModel
            {
                ItemId = x
            }).ToList();
            await _ingestionApiClient.PutItemActionsAsync(deleteItemActions).Timed(_telemetryClient, "IngestionApiPutItemActions");

            _logger.LogInformation("Items in group scheduled to be deleted!");
        }
    }
}

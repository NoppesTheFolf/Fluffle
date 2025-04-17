using Fluffle.Ingestion.Api.Models.ItemActions;
using Fluffle.Ingestion.Worker.ThumbnailStorage;
using Fluffle.Vector.Api.Client;

namespace Fluffle.Ingestion.Worker.ItemActionHandlers;

public class DeleteItemActionHandler : IItemActionHandler
{
    private readonly DeleteItemActionModel _itemAction;
    private readonly IVectorApiClient _vectorApiClient;
    private readonly IThumbnailStorage _thumbnailStorage;
    private readonly ILogger<DeleteItemActionHandler> _logger;

    public DeleteItemActionHandler(DeleteItemActionModel itemAction, IServiceProvider serviceProvider)
    {
        _itemAction = itemAction;
        _vectorApiClient = serviceProvider.GetRequiredService<IVectorApiClient>();
        _thumbnailStorage = serviceProvider.GetRequiredService<IThumbnailStorage>();
        _logger = serviceProvider.GetRequiredService<ILogger<DeleteItemActionHandler>>();
    }

    public async Task RunAsync()
    {
        var itemId = _itemAction.ItemId;

        var item = await _vectorApiClient.GetItemAsync(itemId);
        if (item == null)
        {
            _logger.LogInformation("No item was found on the Vector API to delete, skipping.");
            return;
        }

        _logger.LogInformation("Deleting item from thumbnail storage.");
        await _thumbnailStorage.DeleteAsync(itemId);

        _logger.LogInformation("Deleting item from Vector API.");
        await _vectorApiClient.DeleteItemAsync(itemId);

        _logger.LogInformation("Item deleted!");
    }
}

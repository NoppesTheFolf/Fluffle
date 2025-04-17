using Fluffle.Ingestion.Api.Models.ItemActions;

namespace Fluffle.Ingestion.Api.Client;

public interface IIngestionApiClient
{
    Task PutItemAsync(string itemId, PutItemModel item);

    Task DeleteItemAsync(string itemId);

    Task<ItemActionModel?> DequeueItemActionAsync();

    Task AcknowledgeItemActionAsync(string itemActionId);
}

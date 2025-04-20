using Fluffle.Ingestion.Api.Models.ItemActions;

namespace Fluffle.Ingestion.Api.Client;

public interface IIngestionApiClient
{
    Task<ItemActionModel?> DequeueItemActionAsync();

    Task AcknowledgeItemActionAsync(string itemActionId);
}

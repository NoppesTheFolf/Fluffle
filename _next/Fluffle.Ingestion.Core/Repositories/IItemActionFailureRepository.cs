using Fluffle.Ingestion.Core.Domain.ItemActions;

namespace Fluffle.Ingestion.Core.Repositories;

public interface IItemActionFailureRepository
{
    Task CreateAsync(ItemAction itemAction);
}

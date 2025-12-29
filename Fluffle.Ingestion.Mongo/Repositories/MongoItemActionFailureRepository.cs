using Fluffle.Ingestion.Core.Domain.ItemActions;
using Fluffle.Ingestion.Core.Repositories;

namespace Fluffle.Ingestion.Mongo.Repositories;

internal class MongoItemActionFailureRepository : IItemActionFailureRepository
{
    private readonly MongoContext _context;

    public MongoItemActionFailureRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(ItemAction itemAction)
    {
        await _context.ItemActionFailures.InsertOneAsync(itemAction);
    }
}

using Fluffle.Vector.Core.Domain.Items;
using Fluffle.Vector.Core.Repositories;
using MongoDB.Driver;

namespace Fluffle.Vector.Database.Repositories;

internal class MongoItemVectorsRepository : IItemVectorsRepository
{
    private readonly MongoContext _context;

    public MongoItemVectorsRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task UpsertAsync(ItemVectors itemVectors)
    {
        var filter = Builders<ItemVectors>.Filter.Eq(x => x.ItemVectorsId, itemVectors.ItemVectorsId);
        await _context.ItemVectors.ReplaceOneAsync(filter, itemVectors, new ReplaceOptions
        {
            IsUpsert = true
        });
    }

    public async Task ForEachAsync(string modelId, Action<ItemVectors> action, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ItemVectors>.Filter.Eq(x => x.ItemVectorsId.ModelId, modelId);
        using var cursor = await _context.ItemVectors.Find(filter).ToCursorAsync(cancellationToken);
        foreach (var itemVectors in cursor.ToEnumerable(cancellationToken))
        {
            action(itemVectors);
        }
    }
}

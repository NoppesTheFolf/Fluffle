using Fluffle.Vector.Core.Domain.Items;
using Fluffle.Vector.Core.Repositories;
using MongoDB.Driver;

namespace Fluffle.Vector.Mongo.Repositories;

internal class MongoItemRepository : IItemRepository
{
    private readonly MongoContext _context;

    public MongoItemRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task UpsertAsync(Item item)
    {
        var filter = Builders<Item>.Filter.Eq(x => x.ItemId, item.ItemId);
        await _context.Items.ReplaceOneAsync(filter, item, new ReplaceOptions
        {
            IsUpsert = true
        });
    }

    public async Task<Item?> GetAsync(string itemId)
    {
        var filter = Builders<Item>.Filter.Eq(x => x.ItemId, itemId);
        var item = await _context.Items.Find(filter).FirstOrDefaultAsync();

        return item;
    }

    public async Task DeleteAsync(string itemId)
    {
        var filter = Builders<Item>.Filter.Eq(x => x.ItemId, itemId);
        await _context.Items.DeleteOneAsync(filter);
    }
}

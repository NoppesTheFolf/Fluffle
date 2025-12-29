using Fluffle.Ingestion.Core.Domain.ItemActions;
using Fluffle.Ingestion.Core.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Fluffle.Ingestion.Mongo.Repositories;

internal class MongoItemActionRepository : IItemActionRepository
{
    private readonly MongoContext _context;

    public MongoItemActionRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(ItemAction itemAction)
    {
        itemAction.ItemActionId = ObjectId.GenerateNewId().ToString();
        await _context.ItemActions.InsertOneAsync(itemAction);
    }

    public async Task<ItemAction?> GetByItemIdAsync(string itemId)
    {
        var filter = Builders<ItemAction>.Filter.Eq(new StringFieldDefinition<ItemAction, string>("itemId"), itemId);
        var itemAction = await _context.ItemActions.Find(filter).FirstOrDefaultAsync();

        return itemAction;
    }

    public async Task<ICollection<ItemAction>> GetByGroupIdAsync(string groupId)
    {
        var filter = Builders<ItemAction>.Filter.Eq(new StringFieldDefinition<ItemAction, string>("groupId"), groupId);
        var itemActions = await _context.ItemActions.Find(filter).ToListAsync();

        return itemActions;
    }

    public async Task<ItemAction?> GetHighestPriorityAsync()
    {
        var now = DateTime.UtcNow;
        var filter = Builders<ItemAction>.Filter.Lte(x => x.VisibleWhen, now);
        var sort = Builders<ItemAction>.Sort.Descending(x => x.Priority);
        var itemAction = await _context.ItemActions.Find(filter).Sort(sort).FirstOrDefaultAsync();

        return itemAction;
    }

    public async Task SetVisibleWhenAsync(string itemActionId, DateTime visibleWhen)
    {
        var filter = Builders<ItemAction>.Filter.Eq(x => x.ItemActionId, itemActionId);
        var update = Builders<ItemAction>.Update.Set(x => x.VisibleWhen, visibleWhen);
        await _context.ItemActions.UpdateOneAsync(filter, update);
    }

    public async Task IncrementAttemptCountAsync(string itemActionId)
    {
        var filter = Builders<ItemAction>.Filter.Eq(x => x.ItemActionId, itemActionId);
        var update = Builders<ItemAction>.Update.Inc(x => x.AttemptCount, 1);
        await _context.ItemActions.UpdateOneAsync(filter, update);
    }

    public async Task DeleteAsync(string itemActionId)
    {
        var filter = Builders<ItemAction>.Filter.Eq(x => x.ItemActionId, itemActionId);
        await _context.ItemActions.DeleteOneAsync(filter);
    }

    public async Task DeleteAsync(ICollection<string> itemActionIds)
    {
        var filter = Builders<ItemAction>.Filter.In(x => x.ItemActionId, itemActionIds);
        await _context.ItemActions.DeleteManyAsync(filter);
    }
}

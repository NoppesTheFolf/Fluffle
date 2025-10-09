using Fluffle.Feeder.Bluesky.Core.Domain.Events;
using Fluffle.Feeder.Bluesky.Core.Repositories;
using MongoDB.Driver;

namespace Fluffle.Feeder.Bluesky.Mongo;

internal class MongoBlueskyEventRepository : IBlueskyEventRepository
{
    private readonly MongoContext _context;

    public MongoBlueskyEventRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(BlueskyEvent blueskyEvent)
    {
        await _context.Events.InsertOneAsync(blueskyEvent);
    }

    public async Task<BlueskyEvent?> GetHighestPriorityAsync()
    {
        var filter = Builders<BlueskyEvent>.Filter.Lte(x => x.VisibleWhen, DateTime.UtcNow);
        var sort = Builders<BlueskyEvent>.Sort.Ascending(x => x.UnixTimeMicroseconds);
        var blueskyEvent = await _context.Events.Find(filter).Sort(sort).FirstOrDefaultAsync();

        return blueskyEvent;
    }

    public async Task IncrementAttemptCountAsync(Guid id, DateTime visibleWhen)
    {
        var filter = Builders<BlueskyEvent>.Filter.Eq(x => x.Id, id);
        var update = Builders<BlueskyEvent>.Update.Combine(
            Builders<BlueskyEvent>.Update.Inc(x => x.AttemptCount, 1),
            Builders<BlueskyEvent>.Update.Set(x => x.VisibleWhen, visibleWhen)
        );
        await _context.Events.UpdateOneAsync(filter, update);
    }

    public async Task DeleteAsync(Guid id)
    {
        var filter = Builders<BlueskyEvent>.Filter.Eq(x => x.Id, id);
        await _context.Events.DeleteOneAsync(filter);
    }
}

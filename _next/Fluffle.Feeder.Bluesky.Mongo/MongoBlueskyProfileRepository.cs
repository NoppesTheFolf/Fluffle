using Fluffle.Feeder.Bluesky.Core.Domain;
using Fluffle.Feeder.Bluesky.Core.Repositories;
using MongoDB.Driver;

namespace Fluffle.Feeder.Bluesky.Mongo;

internal class MongoBlueskyProfileRepository : IBlueskyProfileRepository
{
    private readonly MongoContext _context;

    public MongoBlueskyProfileRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(BlueskyProfile profile)
    {
        await _context.Profiles.InsertOneAsync(profile);
    }

    public async Task AddImagePredictionsAsync(string did, IList<BlueskyImagePrediction> imagePredictions)
    {
        var filter = Builders<BlueskyProfile>.Filter.Eq(x => x.Did, did);
        var update = Builders<BlueskyProfile>.Update.AddToSetEach(x => x.ImagePredictions, imagePredictions);

        await _context.Profiles.UpdateOneAsync(filter, update);
    }

    public async Task SetHandleAndDisplayNameAsync(string did, string handle, string? displayName)
    {
        var filter = Builders<BlueskyProfile>.Filter.Eq(x => x.Did, did);
        var update = Builders<BlueskyProfile>.Update
            .Set(x => x.Handle, handle)
            .Set(x => x.DisplayName, displayName);

        await _context.Profiles.UpdateOneAsync(filter, update);
    }

    public async Task<BlueskyProfile?> GetAsync(string did)
    {
        var filter = Builders<BlueskyProfile>.Filter.Eq(x => x.Did, did);
        var profile = await _context.Profiles.Find(filter).FirstOrDefaultAsync();

        return profile;
    }

    public async Task DeleteAsync(string did)
    {
        var filter = Builders<BlueskyProfile>.Filter.Eq(x => x.Did, did);
        await _context.Profiles.DeleteOneAsync(filter);
    }
}

using Fluffle.Feeder.Bluesky.Core.Domain;
using Fluffle.Feeder.Bluesky.Core.Repositories;
using MongoDB.Driver;

namespace Fluffle.Feeder.Bluesky.Mongo;

internal class MongoBlueskyPostRepository : IBlueskyPostRepository
{
    private readonly MongoContext _context;

    public MongoBlueskyPostRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task UpsertAsync(BlueskyPost post)
    {
        var filter = Builders<BlueskyPost>.Filter.Eq(x => x.Id, post.Id);
        await _context.Posts.ReplaceOneAsync(filter, post, new ReplaceOptions { IsUpsert = true });
    }

    public async Task<BlueskyPost?> GetAsync(string did, string rkey)
    {
        var filter = Builders<BlueskyPost>.Filter.Eq(x => x.Id, new BlueskyPostId(did, rkey));
        var post = await _context.Posts.Find(filter).FirstOrDefaultAsync();

        return post;
    }

    public async Task<ICollection<BlueskyPost>> GetByDidAsync(string did)
    {
        var filter = Builders<BlueskyPost>.Filter.Eq(x => x.Id.Did, did);
        var posts = await _context.Posts.Find(filter).ToListAsync();

        return posts;
    }

    public async Task DeleteAsync(string did, string rkey)
    {
        var filter = Builders<BlueskyPost>.Filter.Eq(x => x.Id, new BlueskyPostId(did, rkey));
        await _context.Posts.DeleteOneAsync(filter);
    }

    public async Task DeleteByDidAsync(string did)
    {
        var filter = Builders<BlueskyPost>.Filter.Eq(x => x.Id.Did, did);
        await _context.Posts.DeleteManyAsync(filter);
    }
}

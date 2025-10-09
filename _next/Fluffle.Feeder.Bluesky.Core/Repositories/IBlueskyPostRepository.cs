using Fluffle.Feeder.Bluesky.Core.Domain;

namespace Fluffle.Feeder.Bluesky.Core.Repositories;

public interface IBlueskyPostRepository
{
    Task UpsertAsync(BlueskyPost post);

    Task<BlueskyPost?> GetAsync(string did, string rkey);

    Task<ICollection<BlueskyPost>> GetByDidAsync(string did);

    Task DeleteAsync(string did, string rkey);

    Task DeleteByDidAsync(string did);
}

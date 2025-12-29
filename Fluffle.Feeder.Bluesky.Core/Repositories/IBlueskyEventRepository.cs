using Fluffle.Feeder.Bluesky.Core.Domain.Events;

namespace Fluffle.Feeder.Bluesky.Core.Repositories;

public interface IBlueskyEventRepository
{
    Task CreateAsync(BlueskyEvent blueskyEvent);

    Task<BlueskyEvent?> GetHighestPriorityAsync();

    Task IncrementAttemptCountAsync(Guid id, DateTime visibleWhen);

    Task DeleteAsync(Guid id);
}

using Fluffle.Feeder.Bluesky.Core.Domain;

namespace Fluffle.Feeder.Bluesky.Core.Repositories;

public interface IBlueskyProfileRepository
{
    Task CreateAsync(BlueskyProfile profile);

    Task AddImagePredictionsAsync(string did, IList<BlueskyImagePrediction> imagePredictions);

    Task SetHandleAndDisplayNameAsync(string did, string handle, string? displayName);

    Task<BlueskyProfile?> GetAsync(string did);

    Task DeleteAsync(string did);
}

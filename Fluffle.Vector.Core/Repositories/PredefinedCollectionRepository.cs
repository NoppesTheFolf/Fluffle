using Fluffle.Vector.Core.Domain.Vectors;

namespace Fluffle.Vector.Core.Repositories;

public class PredefinedCollectionRepository : ICollectionRepository
{
    private static readonly Model IntegrationTest = new()
    {
        Id = "integrationTest",
        VectorDimensions = 2
    };

    private static readonly Model ExactMatchV2 = new()
    {
        Id = "exactMatchV2",
        VectorDimensions = 64
    };

    private static readonly Model[] All = [IntegrationTest, ExactMatchV2];
    private static readonly Dictionary<string, Model> Lookup = All.ToDictionary(x => x.Id);

    public Task<Model?> GetAsync(string collectionId)
    {
        if (Lookup.TryGetValue(collectionId, out var model))
        {
            return Task.FromResult<Model?>(model);
        }

        return Task.FromResult<Model?>(null);
    }
}

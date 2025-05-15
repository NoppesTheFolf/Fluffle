using Fluffle.Vector.Core.Domain.Vectors;

namespace Fluffle.Vector.Core.Repositories;

public class PredefinedModelRepository : IModelRepository
{
    private static readonly Model ExactMatchV1 = new()
    {
        Id = "exactMatchV1",
        VectorDimensions = 32
    };

    private static readonly Model[] All = [ExactMatchV1];
    private static readonly Dictionary<string, Model> Lookup = All.ToDictionary(x => x.Id);

    public Task<Model?> GetAsync(string modelId)
    {
        if (Lookup.TryGetValue(modelId, out var model))
        {
            return Task.FromResult<Model?>(model);
        }

        return Task.FromResult<Model?>(null);
    }
}

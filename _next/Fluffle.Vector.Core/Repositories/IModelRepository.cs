using Fluffle.Vector.Core.Domain.Vectors;

namespace Fluffle.Vector.Core.Repositories;

public interface IModelRepository
{
    Task<Model?> GetAsync(string modelId);
}

using Fluffle.Vector.Core.Domain.Vectors;

namespace Fluffle.Vector.Core.Repositories;

public interface ICollectionRepository
{
    Task<Model?> GetAsync(string collectionId);
}

using Noppes.Fluffle.Search.Domain;

namespace Noppes.Fluffle.Search.Business.Repositories;

public interface IImageRepository
{
    Task<IList<Image>> GetAsync(int platformId, long afterChangeId, int limit);
}

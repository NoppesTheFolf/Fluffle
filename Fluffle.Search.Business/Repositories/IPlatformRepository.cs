using Noppes.Fluffle.Search.Domain;

namespace Noppes.Fluffle.Search.Business.Repositories;

public interface IPlatformRepository
{
    Task<ICollection<Platform>> GetAsync();
}

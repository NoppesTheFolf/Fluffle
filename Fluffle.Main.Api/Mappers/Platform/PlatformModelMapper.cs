using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Main.Database.Models;

namespace Noppes.Fluffle.Main.Api.Mappers;

public class PlatformModelMapper : IMapper<Platform, PlatformModel>
{
    public void MapFrom(Platform src, PlatformModel dest)
    {
        dest.Id = src.Id;
        dest.Name = src.Name;
        dest.NormalizedName = src.NormalizedName;
    }
}

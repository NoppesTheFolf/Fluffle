using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Main.Database.Models;

namespace Noppes.Fluffle.Main.Api.Mappers.Sources;

public class OtherSourceModelMapper : IMapper<ContentOtherSource, OtherSourceModel>
{
    public void MapFrom(ContentOtherSource src, OtherSourceModel dest)
    {
        dest.Id = src.Id;
        dest.Location = src.Location;
    }
}

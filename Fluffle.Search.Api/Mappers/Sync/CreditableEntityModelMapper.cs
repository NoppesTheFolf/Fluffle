using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Search.Database.Models;

namespace Noppes.Fluffle.Search.Api.Mappers;

public class CreditableEntityModelMapper : IMapper<CreditableEntitiesSyncModel.CreditableEntityModel, CreditableEntity>
{
    public void MapFrom(CreditableEntitiesSyncModel.CreditableEntityModel src, CreditableEntity dest)
    {
        if (dest.Id != src.Id)
            dest.Id = src.Id;

        dest.PlatformId = src.PlatformId;
        dest.Name = src.Name;
        dest.Type = src.Type;
        dest.Priority = src.Priority;
        dest.ChangeId = src.ChangeId;
    }
}

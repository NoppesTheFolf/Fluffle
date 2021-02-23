using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Search.Database.Models;

namespace Noppes.Fluffle.Search.Api.Mappers
{
    public class CreditableEntityModelMapper : IMapper<CreditableEntitiesSyncModel.CreditableEntityModel, CreditableEntity>
    {
        public void MapFrom(CreditableEntitiesSyncModel.CreditableEntityModel src, CreditableEntity dest)
        {
            dest.Id = dest.Id == default ? src.Id : dest.Id;
            dest.Name = src.Name;
            dest.Type = src.Type;
            dest.ChangeId = src.ChangeId;
        }
    }
}

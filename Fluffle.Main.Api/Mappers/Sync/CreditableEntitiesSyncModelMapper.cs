using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Main.Database.Models;

namespace Noppes.Fluffle.Main.Api.Mappers
{
    public class CreditableEntitiesSyncModelMapper : IMapper<CreditableEntity, CreditableEntitiesSyncModel.CreditableEntityModel>
    {
        public void MapFrom(CreditableEntity src, CreditableEntitiesSyncModel.CreditableEntityModel dest)
        {
            dest.Id = src.Id;
            dest.Name = src.Name;
            dest.Type = src.Type;
            dest.ChangeId = (long)src.ChangeId;
        }
    }
}

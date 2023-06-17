using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Search.Database.Models;

namespace Noppes.Fluffle.Search.Api.Mappers;

public class ImagesSyncModelMapper : IMapper<ImagesSyncModel.ImageModel, Image>
{
    public void MapFrom(ImagesSyncModel.ImageModel src, Image dest)
    {
        if (dest.Id != src.Id)
            dest.Id = src.Id;

        dest.IdOnPlatform = src.IdOnPlatform;
        dest.PlatformId = src.PlatformId;
        dest.ChangeId = src.ChangeId;
        dest.IsDeleted = src.IsDeleted;
        dest.ViewLocation = src.ViewLocation;
        dest.IsSfw = src.IsSfw;
    }
}

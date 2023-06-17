using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Search.Database.Models;

namespace Noppes.Fluffle.Search.Api.Mappers;

public class ImagesSyncModelMapper : IMapper<ImagesSyncModel.ImageModel, Image>, IMapper<ImagesSyncModel.ImageModel.ThumbnailModel, Database.Models.Thumbnail>
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

    public void MapFrom(ImagesSyncModel.ImageModel.ThumbnailModel src, Database.Models.Thumbnail dest)
    {
        dest.Width = src.Width;
        dest.CenterX = src.CenterX;
        dest.Height = src.Height;
        dest.CenterY = src.CenterY;
        dest.Location = src.Location;
    }
}

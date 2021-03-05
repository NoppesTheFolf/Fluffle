using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Search.Database.Models;

namespace Noppes.Fluffle.Search.Api.Mappers
{
    public class ImagesSyncModelMapper : IMapper<ImagesSyncModel.ImageModel, Image>, IMapper<ImagesSyncModel.ImageModel, ImageHash>,
        IMapper<ImagesSyncModel.ImageModel.ThumbnailModel, Thumbnail>
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

        public void MapFrom(ImagesSyncModel.ImageModel src, ImageHash dest)
        {
            if (dest.Id != src.Id)
                dest.Id = src.Id;

            dest.PhashRed64 = src.Hash.PhashRed64;
            dest.PhashGreen64 = src.Hash.PhashGreen64;
            dest.PhashBlue64 = src.Hash.PhashBlue64;
            dest.PhashAverage64 = src.Hash.PhashAverage64;

            dest.PhashRed256 = src.Hash.PhashRed256;
            dest.PhashGreen256 = src.Hash.PhashGreen256;
            dest.PhashBlue256 = src.Hash.PhashBlue256;
            dest.PhashAverage256 = src.Hash.PhashAverage256;

            dest.PhashRed1024 = src.Hash.PhashRed1024;
            dest.PhashGreen1024 = src.Hash.PhashGreen1024;
            dest.PhashBlue1024 = src.Hash.PhashBlue1024;
            dest.PhashAverage1024 = src.Hash.PhashAverage1024;
        }

        public void MapFrom(ImagesSyncModel.ImageModel.ThumbnailModel src, Thumbnail dest)
        {
            dest.Width = src.Width;
            dest.CenterX = src.CenterX;
            dest.Height = src.Height;
            dest.CenterY = src.CenterY;
            dest.Location = src.Location;
        }
    }
}

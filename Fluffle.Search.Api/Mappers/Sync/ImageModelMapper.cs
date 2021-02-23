using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Search.Database.Models;
using System.Linq;

namespace Noppes.Fluffle.Search.Api.Mappers
{
    public class ImageModelMapper : IMapper<ImagesSyncModel.ImageModel, Image>
    {
        public void MapFrom(ImagesSyncModel.ImageModel src, Image dest)
        {
            dest.Id = dest.Id == default ? src.Id : dest.Id;
            dest.IdOnPlatform = src.IdOnPlatform;
            dest.PlatformId = src.PlatformId;
            dest.ChangeId = src.ChangeId;
            dest.IsDeleted = src.IsDeleted;
            dest.ViewLocation = src.ViewLocation;
            dest.IsSfw = src.IsSfw;

            if (src.IsDeleted)
                return;

            dest.ImageHash = new ImageHash
            {
                PhashRed64 = src.Hash.PhashRed64,
                PhashGreen64 = src.Hash.PhashGreen64,
                PhashBlue64 = src.Hash.PhashBlue64,
                PhashAverage64 = src.Hash.PhashAverage64,

                PhashRed256 = src.Hash.PhashRed256,
                PhashGreen256 = src.Hash.PhashGreen256,
                PhashBlue256 = src.Hash.PhashBlue256,
                PhashAverage256 = src.Hash.PhashAverage256,

                PhashRed1024 = src.Hash.PhashRed1024,
                PhashGreen1024 = src.Hash.PhashGreen1024,
                PhashBlue1024 = src.Hash.PhashBlue1024,
                PhashAverage1024 = src.Hash.PhashAverage1024
            };

            dest.Thumbnail = new Thumbnail
            {
                Width = src.Thumbnail.Width,
                CenterX = src.Thumbnail.CenterX,
                Height = src.Thumbnail.Height,
                CenterY = src.Thumbnail.CenterY,
                Location = src.Thumbnail.Location
            };

            dest.ContentCreditableEntities = src.Credits.Select(c => new ContentCreditableEntity
            {
                CreditableEntityId = c
            }).ToList();

            dest.Files = src.Files.Select(f => new ContentFile
            {
                Width = f.Width,
                Height = f.Height,
                Location = f.Location,
                Format = f.Format
            }).ToList();
        }
    }
}

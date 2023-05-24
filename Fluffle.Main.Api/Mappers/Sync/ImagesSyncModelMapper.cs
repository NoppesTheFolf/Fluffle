using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Main.Database.Models;
using System.Linq;

namespace Noppes.Fluffle.Main.Api.Mappers;

public class ImagesSyncModelMapper : IMapper<Image, ImagesSyncModel.ImageModel>
{
    public void MapFrom(Image src, ImagesSyncModel.ImageModel dest)
    {
        dest.Id = src.Id;
        dest.PlatformId = src.PlatformId;
        dest.IdOnPlatform = src.IdOnPlatform;
        dest.ChangeId = (long)src.ChangeId;

        dest.ViewLocation = src.ViewLocation;
        dest.IsDeleted = src.IsDeleted || (src.IsDeleted || src.IsMarkedForDeletion || !src.IsIndexed);
        dest.IsSfw = src.Rating.IsSfw;

        if (dest.IsDeleted)
            return;

        dest.Hash = new ImagesSyncModel.ImageModel.HashModel
        {
            PhashRed64 = src.ImageHash.PhashRed64,
            PhashGreen64 = src.ImageHash.PhashGreen64,
            PhashBlue64 = src.ImageHash.PhashBlue64,
            PhashAverage64 = src.ImageHash.PhashAverage64,
            PhashRed256 = src.ImageHash.PhashRed256,
            PhashGreen256 = src.ImageHash.PhashGreen256,
            PhashBlue256 = src.ImageHash.PhashBlue256,
            PhashAverage256 = src.ImageHash.PhashAverage256,
            PhashRed1024 = src.ImageHash.PhashRed1024,
            PhashGreen1024 = src.ImageHash.PhashGreen1024,
            PhashBlue1024 = src.ImageHash.PhashBlue1024,
            PhashAverage1024 = src.ImageHash.PhashAverage1024
        };

        static ImagesSyncModel.ImageModel.ThumbnailModel ThumbnailModel(Thumbnail thumbnail)
        {
            return new()
            {
                Width = thumbnail.Width,
                CenterX = thumbnail.CenterX,
                Height = thumbnail.Height,
                CenterY = thumbnail.CenterY,
                Location = thumbnail.Location
            };
        }

        dest.Thumbnail = ThumbnailModel(src.Thumbnail);

        dest.Files = src.Files.Select(c => new ImagesSyncModel.ImageModel.FileModel
        {
            Width = c.Width,
            Height = c.Height,
            Format = (FileFormatConstant)c.FileFormatId,
            Location = c.Location
        });

        dest.Credits = src.Credits.Select(c => c.Id);
    }
}

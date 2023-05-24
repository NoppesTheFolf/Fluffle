using Nitranium.PerceptualHashing.Utils;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Thumbnail;
using SerilogTimings;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index;

public class Thumbnailer : ImageConsumer
{
    private static readonly ThumbnailTemplate Template = new(ImageFormatConstant.Jpeg, 300, 75, "normal");

    private readonly FluffleThumbnail _thumbnail;

    public Thumbnailer(FluffleClient fluffleClient, FluffleThumbnail thumbnail) : base(fluffleClient)
    {
        _thumbnail = thumbnail;
    }

    public override Task<ChannelImage> ConsumeAsync(ChannelImage image)
    {
        // Thumbnailing will never fail. The same functionality is used by the hasher and if the
        // image is corrupt or something similar, it will already have crashed while trying to
        // calculate the hash.
        Thumbnail CreateThumbnail(ThumbnailTemplate template)
        {
            using (Operation.Time("[{platformName}, {idOnPlatform}, 3/5] Created {imageType} thumbnail with quality {quality} and size {size}",
                image.Content.PlatformName, image.Content.IdOnPlatform, template.Format, template.Quality, template.Size))
            {
                var thumbnail = Thumbnail(image.File.Location, template);

                return thumbnail;
            }
        }

        image.Thumbnail = CreateThumbnail(Template);

        return Task.FromResult(image);
    }

    private Thumbnail Thumbnail(string sourceLocation, ThumbnailTemplate template)
    {
        var thumbnail = new TemporaryFile();

        var thumbnailResult = _thumbnail.Generate(sourceLocation,
            thumbnail.Location, template.Size, template.Format, template.Quality);

        return new Thumbnail
        {
            Template = template,
            Width = thumbnailResult.Width,
            CenterX = thumbnailResult.CenterX,
            Height = thumbnailResult.Height,
            CenterY = thumbnailResult.CenterY,
            Format = template.Format,
            File = thumbnail
        };
    }
}

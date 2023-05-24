using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index;

public class IndexPublisher : ImageConsumer
{
    public IndexPublisher(FluffleClient fluffleClient) : base(fluffleClient)
    {
    }

    public override async Task<ChannelImage> ConsumeAsync(ChannelImage image)
    {
        var model = new PutImageIndexModel
        {
            Hashes = image.Hashes,
            Thumbnail = new PutContentIndexModel.ThumbnailModel
            {
                Width = image.Thumbnail.Width,
                CenterX = image.Thumbnail.CenterX,
                Height = image.Thumbnail.Height,
                CenterY = image.Thumbnail.CenterY,
                Location = image.Thumbnail.Location,
                Filename = image.Thumbnail.Filename,
                B2FileId = image.Thumbnail.B2FileId
            }
        };

        await LogEx.TimeAsync(async () =>
        {
            await HttpResiliency.RunAsync(() =>
                FluffleClient.IndexImageAsync(image.Content.PlatformName, image.Content.IdOnPlatform, model));
        }, "[{platformName}, {idOnPlatform}, 5/5] Published index", image.Content.PlatformName, image.Content.IdOnPlatform);

        return image;
    }
}

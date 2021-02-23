using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Main.Client;
using System.IO;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index
{
    public class ThumbnailPublisher : ImageConsumer
    {
        private readonly B2ThumbnailStorage _storage;

        public ThumbnailPublisher(FluffleClient fluffleClient, B2ThumbnailStorage storage) : base(fluffleClient)
        {
            _storage = storage;
        }

        public override async Task<ChannelImage> ConsumeAsync(ChannelImage image)
        {
            async Task ThumbnailAsync(Thumbnail thumbnail)
            {
                var response = await LogEx.TimeAsync(async () =>
                {
                    var location = await _storage.PutAsync(() => File.OpenRead(thumbnail.File.Location),
                        image.Content.Platform, image.Content.IdOnPlatform, thumbnail.Template.Discriminator, thumbnail.Format);

                    return location;
                }, "[{platformName}, {idOnPlatform}, 4/5] Submitted thumbnail to BackBlaze B2 storage", image.Content.PlatformName, image.Content.IdOnPlatform);

                thumbnail.Location = response.DownloadUrl;
                thumbnail.Filename = response.FileName;
                thumbnail.B2FileId = response.FileId;

                thumbnail.File.Dispose();
            }

            await ThumbnailAsync(image.Thumbnail);

            return image;
        }
    }
}

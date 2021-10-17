using Flurl.Http;
using Humanizer;
using Nitranium.PerceptualHashing.Utils;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Utils;
using Serilog;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index
{
    public class ImageDownloader : ImageProducer
    {
        private const int TargetSize = 300;
        private static readonly TimeSpan Delay = 5.Minutes();

        private readonly PlatformModel _platform;
        private readonly DownloadClient _downloadClient;

        public ImageDownloader(FluffleClient fluffleClient, PlatformModel platform, DownloadClient downloadClient) : base(fluffleClient)
        {
            _platform = platform;
            _downloadClient = downloadClient;
        }

        public override async Task WorkAsync()
        {
            var images = await HttpResiliency.RunAsync(() =>
                FluffleClient.GetUnprocessedImagesAsync(_platform.Name));

            if (!images.Any())
            {
                Log.Information(
                    "[{platformName}] There are no more images to index. Waiting {delay} before checking again.",
                    _platform.Name, Delay.Humanize());
                await Task.Delay(Delay);
                return;
            }

            foreach (var image in images)
            {
                var channelImage = new ChannelImage
                {
                    Content = image
                };

                channelImage.File = await DownloadAsync(channelImage);

                await ProduceAsync(channelImage);
            }
        }

        private async Task<TemporaryFile> DownloadAsync(ChannelImage ci)
        {
            var temporaryFile = await LogEx.TimeAsync(async () =>
            {
                var orderedImages = ImageSizeHelper.OrderByDownloadPreference(ci.Content.Files, f => f.Width, f => f.Height, TargetSize);

                foreach (var imageFile in orderedImages)
                {
                    var result = await TryDownloadAsync(imageFile.Location, () =>
                    {
                        Log.Warning("[{platformName}, {idOnPlatform}] File not found.", ci.Content.PlatformName, ci.Content.IdOnPlatform);
                        ci.Warnings.Add($"File located at `{imageFile.Location}` could not be downloaded");
                        return Task.CompletedTask;
                    });

                    if (result.success)
                        return result.temporaryFile;
                }

                Log.Error("No files could be downloaded for image with ID {imageId}.", ci.Content.IdOnPlatform);
                ci.Error = "No files could be downloaded.";

                return null;
            }, "[{platformName}, {idOnPlatform}, 1/5] Downloaded image", _platform.Name, ci.Content.IdOnPlatform);

            return temporaryFile;
        }

        private async Task<(bool success, TemporaryFile temporaryFile)> TryDownloadAsync(string url, Func<Task> onNotFoundAsync)
        {
            try
            {
                return (true, await _downloadClient.DownloadAsync(url));
            }
            catch (FlurlHttpException exception)
            {
                if (exception.Call?.Response.StatusCode == (int)HttpStatusCode.NotFound)
                    await onNotFoundAsync();

                return (false, null);
            }
        }
    }
}

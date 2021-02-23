using Flurl.Http;
using Humanizer;
using Nitranium.PerceptualHashing.Utils;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index
{
    public class ImageDownloader : ImageProducer
    {
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
                var orderedImages = OrderByDownloadPreference(ci.Content.Files);

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

        public static IEnumerable<UnprocessedContentModel.FileModel> OrderByDownloadPreference(IEnumerable<UnprocessedContentModel.FileModel> files)
        {
            var imagesByArea = files
                .OrderBy(s => s.Width * s.Height)
                .ToList();

            var preferredImages = imagesByArea
                .Where(s => s.Width >= 300 && s.Height >= 300)
                .ToList();

            var leftOverImages = imagesByArea
                .Except(preferredImages)
                .OrderByDescending(s => s.Width * s.Height);

            return preferredImages.Concat(leftOverImages);
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

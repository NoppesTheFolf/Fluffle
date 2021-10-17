using Flurl.Http;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.TwitterSync.Database.Models;
using Noppes.Fluffle.Utils;
using Serilog;
using SerilogTimings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.TwitterSync.AnalyzeUsers
{
    public class RetrieverImage
    {
        public string TweetId { get; set; }

        public string MediaId { get; set; }

        public string Url { get; set; }

        public ICollection<RetrieverSize> Sizes { get; set; }
    }

    public class RetrieverSize
    {
        public MediaSizeConstant Size { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public ResizeMode Resize { get; set; }
    }

    public interface IImageRetrieverData : IDisposable
    {
        public ICollection<RetrieverImage> Images { get; set; }
        public ICollection<Stream> Streams { get; set; }
        public ICollection<Func<Stream>> OpenStreams { get; set; }

        void IDisposable.Dispose()
        {
            if (Streams == null)
                return;

            foreach (var stream in Streams)
                stream.Dispose();
        }
    }

    public class ImageRetriever<T> : Consumer<T> where T : IImageRetrieverData
    {
        private const int TargetSize = 456;
        private readonly ITwitterDownloadClient _downloadClient;

        public ImageRetriever(ITwitterDownloadClient downloadClient)
        {
            _downloadClient = downloadClient;
        }

        public override async Task<T> ConsumeAsync(T data)
        {
            // Download the images from Twitter
            data.Streams = new List<Stream>();
            data.OpenStreams = new List<Func<Stream>>();
            foreach (var image in data.Images)
            {
                async Task<Stream> DownloadImage(string url, string fallbackUrl)
                {
                    try
                    {
                        return await HttpResiliency.RunAsync(() => _downloadClient.GetStreamAsync(url));
                    }
                    catch (FlurlHttpException e)
                    {
                        if (e.StatusCode != 404)
                            throw;

                        Log.Warning("Attempting to use fallback URL for media with ID {mediaId}...", image.MediaId);
                        return await HttpResiliency.RunAsync(() => _downloadClient.GetStreamAsync(fallbackUrl));
                    }
                }

                var preferredSize = ImageSizeHelper.OrderByDownloadPreference(image.Sizes.Where(s => s.Resize == ResizeMode.Fit), s => s.Width, s => s.Height, TargetSize).First();
                using var _ = Operation.Time("Downloading media with ID {mediaId} for tweet with ID {tweetId} at size {size}", image.MediaId, image.TweetId, preferredSize.Size);
                var url = $"{image.Url}?name={Enum.GetName(preferredSize.Size).ToLowerInvariant()}";
                await using var stream = await DownloadImage(url, image.Url);

                var memoryStream = new MemoryStream();
                data.Streams.Add(memoryStream);

                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                data.OpenStreams.Add(() =>
                {
                    var copy = new MemoryStream();

                    memoryStream.Position = 0;
                    memoryStream.CopyTo(copy);
                    copy.Position = 0;

                    return copy;
                });
            }

            return data;
        }
    }
}

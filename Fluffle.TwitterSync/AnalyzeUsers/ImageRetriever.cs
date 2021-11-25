using Flurl.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        private static readonly int OriginalSize = (int)Math.Floor(Math.Sqrt(int.MaxValue));

        private readonly IServiceProvider _services;
        private readonly ITwitterDownloadClient _downloadClient;

        public ImageRetriever(IServiceProvider services, ITwitterDownloadClient downloadClient)
        {
            _services = services;
            _downloadClient = downloadClient;
        }

        public override async Task<T> ConsumeAsync(T data)
        {
            // Download the images from Twitter
            data.Streams = new List<Stream>();
            data.OpenStreams = new List<Func<Stream>>();
            foreach (var image in data.Images.ToList()) // Make a copy so we can remove items from the original collection
            {
                async Task<Stream> DownloadImageAsync(Stack<(RetrieverSize image, bool isOriginal)> items)
                {
                    if (items.Count == 0)
                        return null;

                    FlurlHttpException exitException = null;
                    while (items.TryPop(out var x))
                    {
                        try
                        {
                            var url = x.isOriginal ? image.Url : $"{image.Url}?name={Enum.GetName(x.Item1.Size).ToLowerInvariant()}";

                            using var _ = Operation.Time("Downloading media with ID {mediaId} for tweet with ID {tweetId} at size {size}", image.MediaId, image.TweetId, x.image.Size);
                            return await HttpResiliency.RunAsync(() => _downloadClient.GetStreamAsync(url));
                        }
                        catch (FlurlHttpException exception)
                        {
                            exitException = exception;

                            Log.Warning("Failed downloading media with ID {mediaId} for tweet with ID {tweetId} at size {size} ({statusCode})", image.MediaId, image.TweetId, x.image.Size, exception.StatusCode);
                        }
                    }

                    if (exitException!.StatusCode is 403 or 404)
                        return null;

                    throw exitException!;
                }

                IEnumerable<(RetrieverSize image, bool isOriginal)> sizes = image.Sizes
                    .Where(s => s.Resize == ResizeMode.Fit)
                    .Select(s => (s, false))
                    .Concat(new[]
                    {
                        (new RetrieverSize
                        {
                            Width = OriginalSize,
                            Height = OriginalSize
                        }, true)
                    });

                var preferredSizes = ImageSizeHelper.OrderByDownloadPreference(sizes, x => x.image.Width, x => x.image.Height, TargetSize);
                var images = new Stack<(RetrieverSize image, bool isOriginal)>(preferredSizes.Reverse());
                await using var stream = await DownloadImageAsync(images);

                if (stream == null)
                {
                    using var scope = _services.CreateScope();
                    await using var context = scope.ServiceProvider.GetRequiredService<TwitterContext>();

                    var media = await context.Media.FirstOrDefaultAsync(m => m.Id == image.MediaId);
                    if (media != null)
                    {
                        Log.Information("Marking media with ID {mediaId} as deleted", media.Id);
                        media.IsNotAvailable = true;

                        await context.SaveChangesAsync();
                    }

                    data.Images.Remove(data.Images.First(i => i.MediaId == image.MediaId));
                    continue;
                }

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

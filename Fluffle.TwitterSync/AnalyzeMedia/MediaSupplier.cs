using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.TwitterSync.AnalyzeUsers;
using Noppes.Fluffle.TwitterSync.Database.Models;
using Noppes.Fluffle.Utils;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.TwitterSync.AnalyzeMedia
{
    public class MediaSupplier : Producer<AnalyzeMediaData>
    {
        private const int BatchSize = 20;
        private static readonly TimeSpan Interval = 1.Minutes();
        private static readonly TimeSpan ReservationTime = 1.Hours();

        private readonly IServiceProvider _services;

        public MediaSupplier(IServiceProvider services)
        {
            _services = services;
        }

        public override async Task WorkAsync()
        {
            using var scope = _services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<TwitterContext>();

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var tweets = await context.Tweets
                .Include(t => t.Media.Where(m => m.MediaType == MediaTypeConstant.Image))
                .ThenInclude(t => t.Sizes.Where(s => s.ResizeMode == ResizeMode.Fit))
                .Where(t => t.ReservedUntil < now && t.ShouldBeAnalyzed && t.AnalyzedAt == null)
                .OrderByDescending(t => t.FavoriteCount)
                .Take(BatchSize)
                .ToListAsync();

            if (tweets.Count == 0)
            {
                Log.Information("Waiting for {interval} before trying to supply media again", Interval);
                await Task.Delay(Interval);
                return;
            }

            var images = tweets.SelectMany(t => t.Media.Select(m => new RetrieverImage
            {
                TweetId = t.Id,
                MediaId = m.Id,
                Url = m.Url,
                Sizes = m.Sizes.Select(s => new RetrieverSize
                {
                    Width = s.Width,
                    Height = s.Height,
                    Resize = s.ResizeMode,
                    Size = s.Size
                }).ToList()
            })).ToList();

            foreach (var tweet in tweets)
            {
                tweet.ReservedUntil = DateTimeOffset.UtcNow.Add(ReservationTime).ToUnixTimeSeconds();
            }
            await context.SaveChangesAsync();

            await ProduceAsync(new AnalyzeMediaData
            {
                TweetIds = tweets.Select(t => t.Id).ToList(),
                Images = images
            });
        }
    }
}

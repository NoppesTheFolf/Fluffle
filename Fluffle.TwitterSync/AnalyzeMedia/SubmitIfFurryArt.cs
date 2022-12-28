using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Sync;
using Noppes.Fluffle.TwitterSync.Database.Models;
using Noppes.Fluffle.Utils;
using SerilogTimings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.TwitterSync.AnalyzeMedia
{
    public class SubmitIfFurryArt : Consumer<AnalyzeMediaData>, IContentMapper<AnalyzeMediaResult>
    {
        private readonly IServiceProvider _services;
        private readonly FluffleClient _client;

        public SubmitIfFurryArt(IServiceProvider services, FluffleClient client)
        {
            _services = services;
            _client = client;
        }

        public override async Task<AnalyzeMediaData> ConsumeAsync(AnalyzeMediaData data)
        {
            using var scope = _services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<TwitterContext>();

            var tweets = await context.Tweets
                .Where(t => data.TweetIds.Contains(t.Id))
                .ToListAsync();

            foreach (var tweet in tweets)
                tweet.AnalyzedAt = DateTimeOffset.UtcNow;

            var predictions = data.Images
                .Select(i => i.MediaId)
                .Zip(data.IsFurryArt, (x, y) => (mediaId: x, isFurryArt: y))
                .Zip(data.Classes, (x, y) => (x.mediaId, x.isFurryArt, classes: y))
                .ToList();

            var analyzedMedia = await context.Media
                .Include(m => m.Sizes)
                .Include(m => m.Tweets)
                .ThenInclude(t => t.CreatedBy)
                .Where(m => data.Images.Select(i => i.MediaId).Contains(m.Id))
                .ToListAsync();

            var results = predictions
                .Join(analyzedMedia, x => x.mediaId, x => x.Id, (x, y) => (media: y, x.isFurryArt, x.classes))
                .ToList();

            foreach (var (media, isFurryArt, _) in results)
                media.IsFurryArt = isFurryArt;

            var contentModels = results
                .Where(r => r.isFurryArt)
                .Select(r => new AnalyzeMediaResult
                {
                    Media = r.media,
                    Tweet = r.media.Tweets.OrderByDescending(t => t.FavoriteCount).First()
                })
                .Select(((IContentMapper<AnalyzeMediaResult>)this).SrcToContent)
                .ToList();

            using (var _ = Operation.Time("Submitting {contentCount} tweets", contentModels.Count))
            {
                await HttpResiliency.RunAsync(async () =>
                {
                    await _client.PutContentAsync("twitter", contentModels);
                });
            }

            await context.SaveChangesAsync();
            return data;
        }

        public string GetId(AnalyzeMediaResult src) => src.Media.Id;

        public string GetReference(AnalyzeMediaResult src) => null;

        public ContentRatingConstant GetRating(AnalyzeMediaResult src) => ContentRatingConstant.Explicit;

        public IEnumerable<PutContentModel.CreditableEntityModel> GetCredits(AnalyzeMediaResult src)
        {
            return new List<PutContentModel.CreditableEntityModel>
            {
                new()
                {
                    Id = src.Tweet.CreatedBy.Id,
                    Name = src.Tweet.CreatedBy.Username
                }
            };
        }

        public string GetViewLocation(AnalyzeMediaResult src) => src.Tweet.Url;

        public IEnumerable<PutContentModel.FileModel> GetFiles(AnalyzeMediaResult src)
        {
            var files = src.Media.Sizes.Select(s => new PutContentModel.FileModel
            {
                Width = s.Width,
                Height = s.Height,
                Location = $"{s.Media.Url}?name={Enum.GetName(s.Size).ToLowerInvariant()}",
                Format = FileFormatHelper.GetFileFormatFromExtension(Path.GetExtension(s.Media.Url)),
            }).ToList();

            files.Add(new PutContentModel.FileModel
            {
                Width = int.MaxValue,
                Height = int.MaxValue,
                Location = src.Media.Url,
                Format = FileFormatHelper.GetFileFormatFromExtension(Path.GetExtension(src.Media.Url))
            });

            return files;
        }

        public IEnumerable<string> GetTags(AnalyzeMediaResult src) => null;

        public MediaTypeConstant GetMediaType(AnalyzeMediaResult src) => src.Media.MediaType;

        public int GetPriority(AnalyzeMediaResult src) => src.Tweet.FavoriteCount;

        public string GetTitle(AnalyzeMediaResult src) => null;

        public string GetDescription(AnalyzeMediaResult src) => null;

        public IEnumerable<string> GetOtherSources(AnalyzeMediaResult src) => null;

        public bool ShouldBeIndexed(AnalyzeMediaResult src) => true;

        public int GetSourceVersion() => throw new InvalidOperationException();
    }
}

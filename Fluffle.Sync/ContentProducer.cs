using Humanizer;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Sync
{
    public abstract class ContentProducer<TContent> : SyncProducer
    {
        protected readonly PlatformModel Platform;
        protected readonly FluffleClient FluffleClient;

        protected ContentProducer(PlatformModel platform, FluffleClient fluffleClient)
        {
            Platform = platform;
            FluffleClient = fluffleClient;
        }

        public override async Task WorkAsync()
        {
            var (syncType, timeToWait) = await GetSyncInfoAsync();

            if (timeToWait != TimeSpan.Zero)
            {
                Log.Information($"Waiting for {{timeToWait}} till {syncType.ToString().ToLowerInvariant()} sync...", timeToWait.Humanize());
                await Task.Delay(timeToWait);
            }

            Func<Task> syncMethodAsync = syncType switch
            {
                SyncTypeConstant.Full => FullSyncAsync,
                SyncTypeConstant.Quick => QuickSyncAsync,
                _ => throw new ArgumentOutOfRangeException(nameof(syncType))
            };

            Log.Information($"Starting {syncType.ToString().ToLowerInvariant()} sync...");
            await syncMethodAsync();

            await HttpResiliency.RunAsync(() =>
                FluffleClient.SignalPlatformSyncAsync(Platform.NormalizedName, syncType));
        }

        protected async Task<(SyncTypeConstant syncType, TimeSpan timeToWait)> GetSyncInfoAsync()
        {
            var syncInfo = await HttpResiliency.RunAsync(() =>
                FluffleClient.GetPlatformSync(Platform.NormalizedName));

            if (syncInfo.Next == null)
            {
                Log.Fatal("Platform doesn't have any ways to sync.");
                Environment.Exit(-1);
            }

            return (syncInfo.Next.Type, syncInfo.Next.TimeToWait);
        }

        protected abstract Task QuickSyncAsync();

        protected abstract Task FullSyncAsync();

        protected async Task SubmitContentAsync(ICollection<TContent> content)
        {
            var contentModels = new List<PutContentModel>();
            foreach (var contentPiece in content)
            {
                var model = new PutContentModel();
                SrcToContent(contentPiece, model);
                contentModels.Add(model);
            }

            await ProduceAsync(contentModels);
        }

        public void SrcToContent(TContent src, PutContentModel dest)
        {
            dest.IdOnPlatform = GetId(src);
            dest.Rating = GetRating(src);
            dest.CreditableEntities = GetCredits(src).ToList();
            dest.ViewLocation = GetViewLocation(src);
            dest.Files = GetFiles(src).ToList();
            dest.Tags = GetTags(src).ToList();
            dest.MediaType = GetMediaType(src);
            dest.Priority = GetPriority(src);
        }

        public abstract string GetId(TContent src);

        public abstract ContentRatingConstant GetRating(TContent src);

        public abstract IEnumerable<PutContentModel.CreditableEntityModel> GetCredits(TContent src);

        public abstract string GetViewLocation(TContent src);

        public abstract IEnumerable<PutContentModel.FileModel> GetFiles(TContent src);

        public abstract IEnumerable<string> GetTags(TContent src);

        public abstract MediaTypeConstant GetMediaType(TContent src);

        public abstract int GetPriority(TContent src);

        protected async Task FlagRangeForDeletionAsync(int exclusiveStart, int inclusiveEnd, ICollection<TContent> posts)
        {
            var platformContentIds = posts
                .Select(GetId)
                .Select(int.Parse)
                .ToList();

            var model = new DeleteContentRangeModel
            {
                ExclusiveStart = exclusiveStart,
                InclusiveEnd = inclusiveEnd,
                ExcludedIds = platformContentIds
            };

            var deletedContentIds = await HttpResiliency.RunAsync(() =>
                FluffleClient.DeleteContentRangeAsync(Platform.NormalizedName, model));

            foreach (var deletedContentId in deletedContentIds)
                Log.Information("Flagged content with ID {deletedContentId} for deletion", deletedContentId);
        }
    }
}

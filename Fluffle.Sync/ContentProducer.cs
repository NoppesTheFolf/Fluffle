using Humanizer;
using Microsoft.Extensions.Hosting;
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
    public abstract class ContentProducer<TContent> : SyncProducer, IContentMapper<TContent>
    {
        protected readonly string Platform;
        protected readonly FluffleClient FluffleClient;
        protected readonly IHostEnvironment Environment;

        protected ContentProducer(PlatformModel platform, FluffleClient fluffleClient, IHostEnvironment environment)
        {
            Platform = platform.NormalizedName;
            FluffleClient = fluffleClient;
            Environment = environment;
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
                FluffleClient.SignalPlatformSyncAsync(Platform, syncType));
        }

        protected async Task<(SyncTypeConstant syncType, TimeSpan timeToWait)> GetSyncInfoAsync()
        {
            var syncInfo = await HttpResiliency.RunAsync(() => FluffleClient.GetPlatformSync(Platform));

            if (syncInfo.Next == null)
            {
                Log.Fatal("Platform doesn't have any ways to sync.");
                System.Environment.Exit(-1);
            }

            return (syncInfo.Next.Type, syncInfo.Next.TimeToWait);
        }

        protected abstract Task QuickSyncAsync();

        protected abstract Task FullSyncAsync();

        protected async Task SubmitContentAsync(ICollection<TContent> content)
        {
            var contentModels = content
                .Select(((IContentMapper<TContent>)this).SrcToContent)
                .ToList();

            await ProduceAsync(contentModels);
        }

        public abstract string GetId(TContent src);

        public abstract ContentRatingConstant GetRating(TContent src);

        public abstract IEnumerable<PutContentModel.CreditableEntityModel> GetCredits(TContent src);

        public abstract string GetViewLocation(TContent src);

        public abstract IEnumerable<PutContentModel.FileModel> GetFiles(TContent src);

        public abstract IEnumerable<string> GetTags(TContent src);

        public abstract MediaTypeConstant GetMediaType(TContent src);

        public abstract int GetPriority(TContent src);

        public abstract string GetTitle(TContent src);

        public abstract string GetDescription(TContent src);

        public abstract IEnumerable<string> GetOtherSources(TContent src);

        public abstract bool ShouldBeIndexed(TContent src);

        public int GetSourceVersion() => SourceVersion;

        public virtual int SourceVersion => 0;

        protected async Task FlagRangeForDeletionAsync(int exclusiveStart, int inclusiveEnd, ICollection<TContent> content)
        {
            var platformContentIds = content
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
                FluffleClient.DeleteContentRangeAsync(Platform, model));

            foreach (var deletedContentId in deletedContentIds)
                Log.Information("Flagged content with ID {deletedContentId} for deletion", deletedContentId);
        }
    }
}

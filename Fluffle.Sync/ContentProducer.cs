using Humanizer;
using Microsoft.Extensions.DependencyInjection;
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

namespace Noppes.Fluffle.Sync;

public abstract class ContentProducer<TContent> : SyncProducer, IContentMapper<TContent>
{
    protected readonly SyncConfiguration SyncConfiguration;
    protected readonly string Platform;
    protected readonly FluffleClient FluffleClient;
    protected readonly IHostEnvironment Environment;

    protected ContentProducer(IServiceProvider services)
    {
        SyncConfiguration = services.GetRequiredService<SyncConfiguration>();
        Platform = SyncConfiguration.Platform.NormalizedName;
        FluffleClient = services.GetRequiredService<FluffleClient>();
        Environment = services.GetRequiredService<IHostEnvironment>();
    }

    public override async Task WorkAsync() => await WorkAsync(SyncConfiguration.SyncType);

    private async Task WorkAsync(SyncTypeConstant syncType)
    {
        var syncInfo = await HttpResiliency.RunAsync(() => FluffleClient.GetPlatformSync(Platform, syncType));
        if (syncInfo.TimeToWait > TimeSpan.Zero)
        {
            var ttw = new[] { syncInfo.TimeToWait, 15.Minutes() }.Min();
            Log.Information($"Waiting for {{timeToWait}} till checking if a {syncType.ToString().ToLowerInvariant()} sync is required again...", ttw.Humanize());

            await Task.Delay(ttw);
            return;
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

    protected abstract Task QuickSyncAsync();

    protected abstract Task FullSyncAsync();

    public abstract Task<TContent> GetContentAsync(string id);

    public async Task SubmitContentAsync(ICollection<TContent> content)
    {
        var contentModels = content
            .Select(((IContentMapper<TContent>)this).SrcToContent)
            .ToList();

        await ProduceAsync(contentModels);
    }

    public abstract string GetId(TContent src);

    public virtual string GetReference(TContent src) => null;

    public abstract ContentRatingConstant GetRating(TContent src);

    public abstract IEnumerable<PutContentModel.CreditableEntityModel> GetCredits(TContent src);

    public abstract string GetViewLocation(TContent src);

    public abstract IEnumerable<PutContentModel.FileModel> GetFiles(TContent src);

    public abstract MediaTypeConstant GetMediaType(TContent src);

    public virtual bool GetHasTransparency(TContent src) => false;

    public abstract int GetPriority(TContent src);

    public abstract string GetTitle(TContent src);

    public abstract bool ShouldBeIndexed(TContent src);

    public async Task FlagForDeletionAsync(string contentId) => await FlagForDeletionAsync(new[] { contentId });

    public async Task FlagForDeletionAsync(ICollection<string> contentIds, bool onlyPrintActuallyDeleted = false)
    {
        var deletedIds = await HttpResiliency.RunAsync(() => FluffleClient.DeleteContentAsync(Platform, contentIds));
        if (onlyPrintActuallyDeleted && !deletedIds.Any())
            return;

        var ids = onlyPrintActuallyDeleted ? deletedIds : contentIds;
        if (ids.Count == 1)
        {
            Log.Information("Content with ID {id} was marked for deletion", ids.First());
            return;
        }

        Log.Information("Content with IDs {ids} were marked for deletion", ids);
    }

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

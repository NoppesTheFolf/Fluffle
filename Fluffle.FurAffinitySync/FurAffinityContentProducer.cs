using Humanizer;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Sync;
using SerilogTimings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Log = Serilog.Log;

namespace Noppes.Fluffle.FurAffinitySync;

public class FurAffinityContentProducer : ContentProducer<FaSubmission>
{
    private readonly SyncStateService<FurAffinitySyncClientState> _syncStateService;

    private FurAffinitySyncClientState _syncState;
    private readonly FurAffinityClient _client;
    private readonly FurAffinitySyncConfiguration _configuration;
    private readonly GetSubmissionScheduler _getSubmissionScheduler;

    public FurAffinityContentProducer(IServiceProvider services,
        SyncStateService<FurAffinitySyncClientState> syncStateService, FurAffinityClient client,
        FurAffinitySyncConfiguration configuration, GetSubmissionScheduler getSubmissionScheduler) : base(services)
    {
        _syncStateService = syncStateService;
        _client = client;
        _configuration = configuration;
        _getSubmissionScheduler = getSubmissionScheduler;
    }

    public override async Task<FaSubmission> GetContentAsync(string id)
    {
        var result = await HttpResiliency.RunAsync(() => _client.GetSubmissionAsync(int.Parse(id)));

        return result?.Result;
    }

    protected override Task QuickSyncAsync() => throw new NotImplementedException();

    private async Task<StopReason> ProcessRetrieverAsync(SequentialSubmissionRetriever retriever, Func<int, Task> onSubmitAsync = null)
    {
        while (true)
        {
            var (stopReason, submissionId, faResult) = await retriever.NextAsync();
            if (stopReason != null)
                return (StopReason)stopReason;

            if (faResult != null)
                await SubmitContentAsync(new List<FaSubmission> { faResult.Result });
            else
                await FlagForDeletionAsync(submissionId.ToString());

            if (onSubmitAsync != null)
                await onSubmitAsync(submissionId);
        }
    }

    protected override async Task FullSyncAsync()
    {
        _syncState = await _syncStateService.InitializeAsync(async state =>
        {
            state.Version = 1;
            state.ArchiveEndId = await FluffleClient.GetMinId(Platform) ?? 38_000_000;
            state.ArchiveStartId = await FluffleClient.GetMaxId(Platform) ?? 37_999_999;
        });

        var buildArchiveTask = Task.Run(async () =>
        {
            var startId = _syncState.Acquire(x => x.ArchiveEndId);
            var retriever = new SequentialSubmissionRetriever(startId, Direction.Backward, null, 0, _getSubmissionScheduler, 2);
            await ProcessRetrieverAsync(retriever, submissionId =>
            {
                _syncState.Acquire(x => x.ArchiveEndId = submissionId);

                return Task.CompletedTask;
            });

            Log.Information("Archive task ended");
        });

        var checkExistingTask = Task.Run(async () =>
        {
            while (true)
            {
                var stopId = await FluffleClient.GetMaxId(Platform);
                if (stopId != null)
                {
                    var stopTime = 30.Days();
                    var startId = _syncState.Acquire(x => x.ArchiveStartId);
                    var retriever = new SequentialSubmissionRetriever(startId, Direction.Forward, stopTime, (int)stopId, _getSubmissionScheduler, 3);

                    await ProcessRetrieverAsync(retriever, submissionId =>
                    {
                        _syncState.Acquire(x => x.ArchiveStartId = submissionId);

                        return Task.CompletedTask;
                    });
                }

                var timeToWait = 5.Minutes();
                Log.Information("Waiting for {time} before retrieving recent submissions again.", timeToWait);
                await Task.Delay(timeToWait);
            }
        });

        var syncStateTask = Task.Run(async () =>
        {
            var timeToWait = 3.Minutes();
            while (true)
            {
                Log.Information("Waiting {time} before storing sync state", timeToWait);
                await Task.Delay(timeToWait);

                await _syncState.AcquireAsync<object>(async _ =>
                {
                    var flushDelay = 3.Seconds();
                    Log.Information("Waiting {time} before storing sync state to allow pending operations to flush...", flushDelay);
                    await Task.Delay(flushDelay);

                    using var __ = Operation.Time("Storing sync state");
                    await HttpResiliency.RunAsync(() => _syncStateService.SyncAsync());

                    return null;
                });
            }
        });

        var recentSubmissionsTask = Task.Run(async () =>
        {
            while (true)
            {
                var recentSubmissions = await _client.GetRecentSubmissions();

                var stopTime = 5.Minutes();
                var stopId = recentSubmissions.OrderByDescending(x => x.Id).First().Id + 1;
                var startId = await FluffleClient.GetMaxId(Platform) ?? 37_999_999;
                var retriever = new SequentialSubmissionRetriever(startId, Direction.Forward, stopTime, stopId, _getSubmissionScheduler, 1);

                var stopReason = await ProcessRetrieverAsync(retriever);
                if (stopReason != StopReason.Time)
                    continue;

                var timeToWait = 5.Minutes();
                Log.Information("Waiting for {time} before retrieving recent submissions again.", timeToWait);
                await Task.Delay(timeToWait);
            }
        });

        // Only the archive task can complete without an error being thrown
        var task = await Task.WhenAny(recentSubmissionsTask, buildArchiveTask, syncStateTask, checkExistingTask);
        if (task == buildArchiveTask && task.Exception == null)
            task = await Task.WhenAny(recentSubmissionsTask, syncStateTask, checkExistingTask);

        throw task.Exception!;
    }

    public override string GetId(FaSubmission src) => src.Id.ToString();

    public override ContentRatingConstant GetRating(FaSubmission src)
    {
        return src.Rating switch
        {
            FaSubmissionRating.General => ContentRatingConstant.Safe,
            FaSubmissionRating.Mature => ContentRatingConstant.Questionable,
            FaSubmissionRating.Adult => ContentRatingConstant.Explicit,
            _ => throw new ArgumentOutOfRangeException(nameof(src))
        };
    }

    public override IEnumerable<PutContentModel.CreditableEntityModel> GetCredits(FaSubmission src)
    {
        yield return new PutContentModel.CreditableEntityModel
        {
            Id = src.Owner.Id,
            Name = src.Owner.Name,
            Type = CreditableEntityType.Owner
        };
    }

    public override string GetViewLocation(FaSubmission src) => src.ViewLocation.AbsoluteUri;

    public override IEnumerable<PutContentModel.FileModel> GetFiles(FaSubmission src)
    {
        PutContentModel.FileModel Thumbnail(int targetMax)
        {
            var thumbnail = src.GetThumbnail(targetMax);

            return new PutContentModel.FileModel
            {
                Location = thumbnail.Location.AbsoluteUri,
                Format = FileFormatHelper.GetFileFormatFromExtension(Path.GetExtension(thumbnail.Location.AbsoluteUri)),
                Width = thumbnail.Width,
                Height = thumbnail.Height
            };
        }

        return new List<PutContentModel.FileModel>
        {
            new()
            {
                Location = src.FileLocation.AbsoluteUri,
                Width = src.Size?.Width ?? -1,
                Height = src.Size?.Height ?? -1,
                Format = FileFormatHelper.GetFileFormatFromExtension(Path.GetExtension(src.FileLocation.AbsoluteUri), FileFormatConstant.Binary)
            },
            Thumbnail(200),
            Thumbnail(300),
            Thumbnail(400),
            Thumbnail(600),
            Thumbnail(800)
        };
    }

    public override MediaTypeConstant GetMediaType(FaSubmission src)
    {
        var fileFormat = FileFormatHelper.GetFileFormatFromExtension(Path.GetExtension(src.FileLocation.AbsoluteUri), FileFormatConstant.Binary);

        return fileFormat switch
        {
            FileFormatConstant.Png => MediaTypeConstant.Image,
            FileFormatConstant.Jpeg => MediaTypeConstant.Image,
            FileFormatConstant.WebP => MediaTypeConstant.Image,
            FileFormatConstant.Gif => MediaTypeConstant.AnimatedImage,
            FileFormatConstant.WebM => MediaTypeConstant.Video,
            _ => MediaTypeConstant.Other,
        };
    }

    public override int GetPriority(FaSubmission src) => src.Stats.Views + src.Stats.Favorites * 4 + src.Stats.Comments * 8;

    public override string GetTitle(FaSubmission src) => src.Title;

    public override IEnumerable<string> GetOtherSources(FaSubmission src) => null;

    public static readonly IReadOnlySet<FaSubmissionCategory> DisallowedCategories = new HashSet<FaSubmissionCategory>
    {
        FaSubmissionCategory.Crafting,
        FaSubmissionCategory.Fursuiting,
        FaSubmissionCategory.Photography,
        FaSubmissionCategory.FoodRecipes,
        FaSubmissionCategory.Sculpting,
        FaSubmissionCategory.Skins,
        FaSubmissionCategory.Handhelds,
        FaSubmissionCategory.Resources,
        FaSubmissionCategory.Adoptables,
        FaSubmissionCategory.Auctions,
        FaSubmissionCategory.Contests,
        FaSubmissionCategory.CurrentEvents,
        FaSubmissionCategory.Stockart,
        FaSubmissionCategory.Screenshots,
        FaSubmissionCategory.YchSale
    };

    public static readonly IReadOnlySet<FaSubmissionType> DisallowedTypes = new HashSet<FaSubmissionType>
    {
        FaSubmissionType.Tutorials
    };

    public override bool ShouldBeIndexed(FaSubmission src)
    {
        if (src.Category != null && DisallowedCategories.Contains((FaSubmissionCategory)src.Category))
        {
            Log.Information("Not indexing submission with ID {id} due to its category ({category})", src.Id, src.Category);
            return false;
        }

        if (src.Type != null && DisallowedTypes.Contains((FaSubmissionType)src.Type))
        {
            Log.Information("Not indexing submission with ID {id} due to its type ({category})", src.Id, src.Type);
            return false;
        }

        return true;
    }
}

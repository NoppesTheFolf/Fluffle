using Humanizer;
using Microsoft.Extensions.Hosting;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Inkbunny.Client;
using Noppes.Fluffle.Inkbunny.Client.Models;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Sync;
using Serilog;
using SerilogTimings;
using System.Collections.Concurrent;

namespace Noppes.Fluffle.Inkbunny.Sync;

public class InkbunnyContentProducer : ContentProducer<FileForSubmission>
{
    private readonly IInkbunnyClient _inkbunnyClient;
    private readonly SyncStateService<InkbunnySyncState> _syncStateService;

    private InkbunnySyncState? _syncState;

    public InkbunnyContentProducer(PlatformModel platform, FluffleClient fluffleClient, IHostEnvironment environment,
        IInkbunnyClient inkbunnyClient, SyncStateService<InkbunnySyncState> syncStateService) : base(platform, fluffleClient, environment)
    {
        _inkbunnyClient = inkbunnyClient;
        _syncStateService = syncStateService;
    }

    protected override Task QuickSyncAsync()
    {
        throw new NotImplementedException();
    }

    protected override async Task FullSyncAsync()
    {
        _syncState = await _syncStateService.InitializeAsync(x =>
        {
            x.Version = 1;
            x.LatestId = -1;

            return Task.CompletedTask;
        });

        var start = _syncState.LatestId - 2000;
        start = start < 1 ? 1 : start;

        var retrieveUntilId = await GetLatestIdAsync();
        do
        {
            var ids = Enumerable.Range(start, InkbunnyConstants.MaximumSubmissionsPerCall).Select(x => x.ToString()).ToList();
            ICollection<Submission> submissions;
            using (Operation.Time("Retrieving {n} submissions starting from ID {id}", InkbunnyConstants.MaximumSubmissionsPerCall, start))
                submissions = (await _inkbunnyClient.GetSubmissionsAsync(ids)).Submissions;

            var idsOnFluffle = new ConcurrentBag<string>();
            await Parallel.ForEachAsync(ids, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (x, _) =>
            {
                var fluffleIds = await HttpResiliency.RunAsync(() => FluffleClient.SearchContentAsync(Platform, x + "-"));
                foreach (var fluffleId in fluffleIds)
                    idsOnFluffle.Add(fluffleId);
            });

            var idsInBatch = submissions.SelectMany(x => x.Files.Select(y => GetId(x.Id, y.Id))).ToList();
            var idsToDelete = idsOnFluffle.Except(idsInBatch).ToList();
            if (idsToDelete.Any())
                await FlagForDeletionAsync(idsToDelete);

            // Filter out posts not older than 15 minutes
            submissions = submissions
                .Where(x => DateTimeOffset.UtcNow.Subtract(x.CreatedWhen) > 15.Minutes())
                .ToList();

            // Make sure none of the submissions contain any files of which the submission parent
            // does not match. Just a piece of mind check
            if (submissions.Any(x => x.Files.Any(y => y.SubmissionId != x.Id)))
                throw new InvalidOperationException("There was at least one submission of which one or more of the files did not refer to the submission as parent.");

            if (submissions.Any())
            {
                var content = submissions.SelectMany(x => x.Files.Select(y => new FileForSubmission(x, y))).ToList();
                await SubmitContentAsync(content);
            }

            if (ids.Contains(retrieveUntilId))
            {
                Log.Information("Stopping because latest ID has been retrieved.");
                break;
            }

            start += 100;
        } while (true);

        _syncState.LatestId = int.Parse(retrieveUntilId);
        await _syncStateService.SyncAsync();
    }

    private async Task<string> GetLatestIdAsync()
    {
        var submissions = await _inkbunnyClient.SearchSubmissionsAsync(SubmissionSearchOrder.CreateDatetime);
        var maxId = submissions.Submissions
            .Select(x => int.Parse(x.Id))
            .MaxBy(x => x);

        return maxId.ToString();
    }

    public override async Task<FileForSubmission> GetContentAsync(string id)
    {
        var idParts = id.Split('-');
        var submissionId = idParts[0];
        var fileId = idParts[1];

        var response = await _inkbunnyClient.GetSubmissionsAsync(new[] { submissionId });

        // Check if the submission has been deleted
        var submission = response.Submissions.SingleOrDefault();
        if (submission == null)
            return null;

        // Check if the specific file for said submission has been deleted
        var file = submission.Files.SingleOrDefault(x => x.Id == fileId);
        if (file == null)
            return null;

        return new FileForSubmission(submission, file);
    }

    public override string GetId(FileForSubmission src) => GetId(src.Submission.Id, src.File.Id);

    private static string GetId(string submissionId, string fileId) => $"{submissionId}-{fileId}";

    public override ContentRatingConstant GetRating(FileForSubmission src)
    {
        return src.Submission.Rating switch
        {
            SubmissionRating.General => ContentRatingConstant.Safe,
            SubmissionRating.Mature => ContentRatingConstant.Questionable,
            SubmissionRating.Adult => ContentRatingConstant.Explicit,
            _ => throw new ArgumentOutOfRangeException(nameof(src.Submission.Rating))
        };
    }

    public override IEnumerable<PutContentModel.CreditableEntityModel> GetCredits(FileForSubmission src)
    {
        return new List<PutContentModel.CreditableEntityModel>
        {
            new()
            {
                Id = src.Submission.UserId,
                Name = src.Submission.Username,
                Type = CreditableEntityType.Owner
            }
        };
    }

    public override string GetViewLocation(FileForSubmission src)
    {
        var baseUrl = $"https://inkbunny.net/s/{src.Submission.Id}";

        if (src.File.Order != 0)
            baseUrl += $"-p{src.File.Order + 1}";

        return baseUrl;
    }

    public override IEnumerable<PutContentModel.FileModel> GetFiles(FileForSubmission src)
    {
        var file = src.File;

        (string? url, int? width, int? height)[] fileCombinations =
        {
            (file.FullFileUrl, file.FullFileWidth, file.FullFileHeight),
            (file.ScreenFileUrl, file.ScreenFileWidth, file.ScreenFileHeight),
            (file.PreviewFileUrl, file.PreviewFileWidth, file.PreviewFileHeight),
            (file.NonCustomHugeThumbnailUrl, file.NonCustomHugeThumbnailWidth, file.NonCustomHugeThumbnailHeight),
            (file.NonCustomLargeThumbnailUrl, file.NonCustomLargeThumbnailWidth, file.NonCustomLargeThumbnailHeight),
            (file.NonCustomMediumThumbnailUrl, file.NonCustomMediumThumbnailWidth, file.NonCustomMediumThumbnailHeight)
        };

        var files = fileCombinations.Where(x => x.url != null).Select(x => new PutContentModel.FileModel
        {
            Location = x.url,
            Width = x.width ?? -1,
            Height = x.height ?? -1,
            Format = FileFormatHelper.GetFileFormatFromExtension(Path.GetExtension(x.url)),
        }).DistinctBy(x => x.Location);

        return files;
    }

    public override IEnumerable<string> GetTags(FileForSubmission src) => src.Submission.Keywords.Select(x => x.Name);

    public override MediaTypeConstant GetMediaType(FileForSubmission src)
    {
        var fileFormat = FileFormatHelper.GetFileFormatFromMimeType(src.File.MimeType);

        return fileFormat switch
        {
            FileFormatConstant.Gif => MediaTypeConstant.AnimatedImage,
            FileFormatConstant.Png => MediaTypeConstant.Image,
            FileFormatConstant.Jpeg => MediaTypeConstant.Image,
            FileFormatConstant.Rtf => MediaTypeConstant.Other,
            FileFormatConstant.Txt => MediaTypeConstant.Other,
            FileFormatConstant.Swf => MediaTypeConstant.Other,
            FileFormatConstant.Mp3 => MediaTypeConstant.Other,
            FileFormatConstant.Doc => MediaTypeConstant.Other,
            FileFormatConstant.Mp4 => MediaTypeConstant.Video,
            FileFormatConstant.Flv => MediaTypeConstant.Video,
            _ => throw new ArgumentOutOfRangeException(nameof(fileFormat), fileFormat, "File format could not be mapped to media type.")
        };
    }

    public override int GetPriority(FileForSubmission src) => src.Submission.Views;

    public override string GetTitle(FileForSubmission src) => src.Submission.Title;

    public override string GetDescription(FileForSubmission src) => src.Submission.Description;

    public override IEnumerable<string> GetOtherSources(FileForSubmission src) => Array.Empty<string>();

    public override bool ShouldBeIndexed(FileForSubmission src) => true;
}

using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Sync;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurryNetworkSync;

public class FurryNetworkContentProducer : ContentProducer<FnSubmission>
{
    private readonly FurryNetworkClient _client;

    public FurryNetworkContentProducer(IServiceProvider services, FurryNetworkClient client) : base(services)
    {
        _client = client;
    }

    public override async Task<FnSubmission> GetContentAsync(string id)
    {
        var submission = await _client.GetSubmissionAsync(int.Parse(id));

        return submission;
    }

    protected override Task FullSyncAsync() => SyncAsync();

    protected override Task QuickSyncAsync() => SyncAsync(FurryNetworkClient.MaximumSubmissionsPerSearch * 15);

    private async Task SyncAsync(int from = -1)
    {
        await foreach (var submissions in EnumerateSubmissionsAsync(from))
            await SubmitContentAsync(submissions.ToList());

        // TODO: Currently there is no way of determining whether a submission is deleted or not
    }

    private async IAsyncEnumerable<IEnumerable<FnSubmission>> EnumerateSubmissionsAsync(int from = -1)
    {
        // We don't know where to start without knowing the total number of submissions first
        if (from < 0)
        {
            var firstSubmissionsPage = await HttpResiliency.RunAsync(() => _client.SearchAsync());
            from = firstSubmissionsPage.Total / FurryNetworkClient.MaximumSubmissionsPerSearch * 72;
        }

        while (true)
        {
            var result = await LogEx.TimeAsync(async () =>
            {
                return await HttpResiliency.RunAsync(() =>
                    _client.SearchAsync(from, FurryNetworkClient.MaximumSubmissionsPerSearch));
            }, "Retrieving submissions from {from}", from);

            yield return result.Before.Concat(result.Hits).Concat(result.After);

            // This means we've retrieved the last page
            if (from == 0)
                break;

            if (from - FurryNetworkClient.MaximumSubmissionsPerSearch < 0)
            {
                from = 0;
                continue;
            }

            from -= FurryNetworkClient.MaximumSubmissionsPerSearch;
        }
    }

    public override IEnumerable<PutContentModel.CreditableEntityModel> GetCredits(FnSubmission src)
    {
        yield return new PutContentModel.CreditableEntityModel
        {
            Id = src.Character.Id.ToString(),
            Name = src.Character.DisplayName ?? src.Character.Name,
            Type = CreditableEntityType.Owner
        };
    }

    public override IEnumerable<PutContentModel.FileModel> GetFiles(FnSubmission src)
    {
        static PutContentModel.FileModel UrlToModel(string url)
        {
            var extension = Path.GetExtension(url);
            var fileName = Path.GetFileNameWithoutExtension(url).ToLowerInvariant();

            var dimensions = fileName.Split('x');
            var width = int.Parse(dimensions[0]);
            var height = int.Parse(dimensions[1]);

            return new()
            {
                Location = url,
                Format = FileFormatHelper.GetFileFormatFromExtension(extension),
                Width = width,
                Height = height
            };
        }

        // Furry Network doesn't provide any information whatsoever about the actual dimensions
        // of the image. We therefore have no idea what dimensions of the images are when we
        // download them. Just be be sure, we only include medium and large thumbnails as these
        // are generally large enough to generate a proper thumbnail and hash
        yield return UrlToModel(src.Images.Medium);
        yield return UrlToModel(src.Images.Large);
    }

    public override string GetId(FnSubmission src) => src.Id.ToString();

    public override MediaTypeConstant GetMediaType(FnSubmission src)
    {
        var fileFormat = FileFormatHelper.GetFileFormatFromMimeType(src.ContentType);

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

    // We use the number of favorites as a way to determine its indexing priority
    public override int GetPriority(FnSubmission src) => src.Favorites;

    public override ContentRatingConstant GetRating(FnSubmission src)
    {
        return src.Rating switch
        {
            FnSubmissionRating.General => ContentRatingConstant.Safe,
            FnSubmissionRating.Mature => ContentRatingConstant.Explicit,
            FnSubmissionRating.Explicit => ContentRatingConstant.Explicit,
            _ => throw new ArgumentOutOfRangeException(nameof(src))
        };
    }

    public override string GetViewLocation(FnSubmission src) => $"https://furrynetwork.com/{src.RecordType}/{src.Id}";

    public override string GetTitle(FnSubmission src) => src.Title;

    public override string GetDescription(FnSubmission src) => src.Description;

    public override IEnumerable<string> GetOtherSources(FnSubmission src) => null;

    public override bool ShouldBeIndexed(FnSubmission src) => true;
}

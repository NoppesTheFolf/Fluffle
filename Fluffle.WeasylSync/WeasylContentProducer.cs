using Microsoft.Extensions.Hosting;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Sync;
using Noppes.Fluffle.Weasyl;
using Noppes.Fluffle.Weasyl.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.WeasylSync
{
    internal class WeasylContentProducer : ContentProducer<Submission>
    {
        private readonly WeasylClient _weasylClient;

        public WeasylContentProducer(PlatformModel platform, FluffleClient fluffleClient,
            IHostEnvironment environment, WeasylClient weasylClient) : base(platform, fluffleClient, environment)
        {
            _weasylClient = weasylClient;
        }

        public override async Task<Submission> GetContentAsync(string id)
        {
            var submission = await HttpResiliency.RunAsync(() => _weasylClient.GetSubmissionAsync(int.Parse(id)));

            return submission?.Media.Submission == null ? null : submission;
        }

        protected override async Task QuickSyncAsync()
        {
            var fontPageSubmissions = await HttpResiliency.RunAsync(() => _weasylClient.GetFrontPageAsync());

            var minId = await HttpResiliency.RunAsync(() => FluffleClient.GetMaxId(Platform)) ?? -1;
            var maxId = fontPageSubmissions.Max(s => s.SubmitId);
            for (var id = minId + 1; id <= maxId; id++)
            {
                var submission = await LogEx.TimeAsync(async () =>
                {
                    return await HttpResiliency.RunAsync(() => _weasylClient.GetSubmissionAsync(id));
                }, "Retrieving submission with ID {id}", id);

                if (submission?.Media.Submission == null)
                    continue;

                await SubmitContentAsync(new[] { submission });
            }
        }

        protected override Task FullSyncAsync() => throw new NotImplementedException();

        public override string GetId(Submission src) => src.SubmitId.ToString();

        public override ContentRatingConstant GetRating(Submission src) => src.Rating switch
        {
            SubmissionRating.General => ContentRatingConstant.Safe,
            SubmissionRating.Moderate => ContentRatingConstant.Questionable,
            SubmissionRating.Mature => ContentRatingConstant.Questionable,
            SubmissionRating.Explicit => ContentRatingConstant.Explicit,
            _ => throw new ArgumentOutOfRangeException(nameof(src))
        };

        public override IEnumerable<PutContentModel.CreditableEntityModel> GetCredits(Submission src)
        {
            yield return new PutContentModel.CreditableEntityModel
            {
                Id = src.OwnerLogin,
                Name = src.Owner,
                Type = CreditableEntityType.Owner
            };
        }

        public override string GetViewLocation(Submission src) => src.Link.AbsoluteUri;

        public override IEnumerable<PutContentModel.FileModel> GetFiles(Submission src)
        {
            var submissionMedia = (src.Media.Cover ?? src.Media.Submission)[0];

            yield return new PutContentModel.FileModel
            {
                Width = -1,
                Height = -1,
                Format = FileFormatHelper.GetFileFormatFromExtension(Path.GetExtension(submissionMedia.Url.AbsoluteUri)),
                Location = submissionMedia.Url.AbsoluteUri
            };
        }

        public override IEnumerable<string> GetTags(Submission src) => src.Tags;

        public override MediaTypeConstant GetMediaType(Submission src)
        {
            var fileFormat = FileFormatHelper.GetFileFormatFromExtension(Path.GetExtension(src.Media.Submission[0].Url.AbsoluteUri));

            return fileFormat switch
            {
                FileFormatConstant.Png => MediaTypeConstant.Image,
                FileFormatConstant.Jpeg => MediaTypeConstant.Image,
                FileFormatConstant.WebP => MediaTypeConstant.Image,
                FileFormatConstant.Gif => MediaTypeConstant.AnimatedImage,
                FileFormatConstant.WebM => MediaTypeConstant.Video,
                _ => MediaTypeConstant.Other
            };
        }

        public override int GetPriority(Submission src) => src.FavoritesCount;

        public override string GetTitle(Submission src) => src.Title;

        public override string GetDescription(Submission src) => src.Description;

        public override IEnumerable<string> GetOtherSources(Submission src) => null;

        public override bool ShouldBeIndexed(Submission src) => true;
    }
}

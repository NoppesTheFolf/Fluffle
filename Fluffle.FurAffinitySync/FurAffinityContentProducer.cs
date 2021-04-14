using Humanizer;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Sync;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurAffinitySync
{
    public class FurAffinityContentProducer : ContentProducer<FaSubmission>
    {
        private static readonly IReadOnlySet<FaSubmissionCategory> DisallowedCategories = new HashSet<FaSubmissionCategory>
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

        private static readonly TimeSpan CheckInterval = 5.Minutes();
        private readonly FurAffinityClient _client;

        public override int SourceVersion => 1;

        public FurAffinityContentProducer(PlatformModel platform, FluffleClient fluffleClient, FurAffinityClient client)
            : base(platform, fluffleClient)
        {
            _client = client;
        }

        protected override Task QuickSyncAsync() => throw new NotImplementedException();

        protected override async Task FullSyncAsync()
        {
            var maxId = await FluffleClient.GetMinId(Platform);
            var id = maxId ?? 38_000_001;

            for (var i = id - 1; i > 0; i--)
            {
                var getSubmissionResult = await LogEx.TimeAsync(async () =>
                {
                    return await HttpResiliency.RunAsync(() => _client.GetSubmissionAsync(i));
                }, "Retrieving submission with ID {id}", i);

                if (getSubmissionResult == null)
                    continue;

                var submission = getSubmissionResult.Result;

                var isDisallowed = false;
                if (DisallowedCategories.Contains(submission.Category))
                {
                    Log.Information("Skipping submission with ID {id} due to its category ({category})",
                        submission.Id, submission.Category);
                    isDisallowed = true;
                }

                if (!isDisallowed && DisallowedTypes.Contains(submission.Type))
                {
                    Log.Information("Skipping submission with ID {id} due to its type ({category})",
                        submission.Id, submission.Type);
                    isDisallowed = true;
                }

                if (!isDisallowed)
                    await SubmitContentAsync(new List<FaSubmission> { submission });

                if (getSubmissionResult.Stats.Registered < FurAffinityClient.BotThreshold)
                    continue;

                bool allowedToContinue;
                do
                {
                    Log.Information("No bots allowed at this moment. Waiting for {time} before checking again.", CheckInterval.Humanize());
                    await Task.Delay(CheckInterval);

                    allowedToContinue = await FluffleClient.GetFaBotsAllowed();
                } while (!allowedToContinue);
                Log.Information("Bots allowed again, continuing full sync...");
            }
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
                    Width = src.Width,
                    Height = src.Height,
                    Format = FileFormatHelper.GetFileFormatFromExtension(Path.GetExtension(src.FileLocation.AbsoluteUri))
                },
                Thumbnail(200),
                Thumbnail(300),
                Thumbnail(400),
                Thumbnail(600),
                Thumbnail(800)
            };
        }

        public override IEnumerable<string> GetTags(FaSubmission src) => src.Tags;

        public override MediaTypeConstant GetMediaType(FaSubmission src)
        {
            var fileFormat = FileFormatHelper.GetFileFormatFromExtension(Path.GetExtension(src.FileLocation.AbsoluteUri));

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

        public override string GetDescription(FaSubmission src) => src.Description;
    }
}

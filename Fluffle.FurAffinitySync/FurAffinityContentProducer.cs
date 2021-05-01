using Humanizer;
using Microsoft.Extensions.Hosting;
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
        private readonly SyncStateService<FurAffinitySyncClientState> _syncStateService;

        private ArchiveStrategy _archiveStrategy;
        private PopularArtistsStrategy _popularArtistsStrategy;
        private FurAffinityContentProducerStrategy _strategy;

        private FurAffinitySyncClientState _syncState;
        private readonly FurAffinityClient _client;

        public override int SourceVersion => 2;

        public FurAffinityContentProducer(PlatformModel platform, FluffleClient fluffleClient,
            IHostEnvironment environment, SyncStateService<FurAffinitySyncClientState> syncStateService, FurAffinityClient client) : base(platform, fluffleClient, environment)
        {
            _syncStateService = syncStateService;
            _client = client;
        }

        protected override Task QuickSyncAsync() => throw new NotImplementedException();

        protected override async Task FullSyncAsync()
        {
            _syncState = await _syncStateService.InitializeAsync(async state =>
            {
                state.Version = 1;
                state.ArchiveEndId = await FluffleClient.GetMinId(Platform) ?? 38_000_001;
                state.ArchiveStartId = await FluffleClient.GetMaxId(Platform) ?? 38_000_000;
            });
            _archiveStrategy = new ArchiveStrategy(FluffleClient, _client, _syncState);
            _popularArtistsStrategy = new PopularArtistsStrategy(FluffleClient, _client, _syncState);
            _strategy = _archiveStrategy;

            for (var i = 1; ; i++)
            {
                var result = await _strategy.NextAsync();

                if (result?.FaResult != null)
                    await SubmitContentAsync(new List<FaSubmission> { result.FaResult.Result });

                if (i % 10 == 0)
                {
                    await LogEx.TimeAsync(async () =>
                    {
                        await HttpResiliency.RunAsync(() => _syncStateService.SyncAsync());
                    }, "Stored sync state");
                }

                if (result == null)
                {
                    if (_strategy is PopularArtistsStrategy)
                    {
                        await LogEx.TimeAsync(async () =>
                        {
                            await HttpResiliency.RunAsync(() => _syncStateService.SyncAsync());
                        }, "Stored sync state");

                        Log.Information("Switching back to archive strategy");
                        _strategy = _archiveStrategy;

                        continue;
                    }

                    Log.Information("Nothing more to do, waiting...");
                    await Task.Delay(15.Minutes());
                    continue;
                }

                if (i % 7000 == 0 && _strategy is ArchiveStrategy)
                {
                    Log.Information("Switching to popular artists strategy");
                    _strategy = _popularArtistsStrategy;
                }

                if (result.FaResult == null || result.FaResult.Stats.Registered < FurAffinityClient.BotThreshold)
                    continue;

                if (Environment.IsDevelopment())
                    continue;

                bool allowedToContinue;
                do
                {
                    Log.Information("No bots allowed at this moment. Waiting for {time} before checking again.", CheckInterval.Humanize());
                    await Task.Delay(CheckInterval);

                    allowedToContinue = await FluffleClient.GetFaBotsAllowedAsync();
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
                    Width = src.Size?.Width ?? -1,
                    Height = src.Size?.Height ?? -1,
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

        public override bool ShouldBeIndexed(FaSubmission src)
        {
            if (DisallowedCategories.Contains(src.Category))
            {
                Log.Information("Not indexing submission with ID {id} due to its category ({category})", src.Id, src.Category);
                return false;
            }

            if (DisallowedTypes.Contains(src.Type))
            {
                Log.Information("Not indexing submission with ID {id} due to its type ({category})", src.Id, src.Type);
                return false;
            }

            return true;
        }
    }
}

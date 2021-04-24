using Noppes.E621;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Sync;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Noppes.Fluffle.E621Sync
{
    internal class E621ContentProducer : ContentProducer<Post>
    {
        private readonly IE621Client _e621Client;

        public E621ContentProducer(PlatformModel platform, FluffleClient fluffleClient,
            IHostEnvironment environment, IE621Client e621Client) : base(platform, fluffleClient, environment)
        {
            _e621Client = e621Client;
        }

        protected override async Task FullSyncAsync() => await SyncFromId(0);

        protected override async Task QuickSyncAsync()
        {
            var maxId = await HttpResiliency.RunAsync(() => FluffleClient.GetMaxId(Platform));

            var id = maxId ?? 0;
            id -= 15 * E621Constants.PostsMaximumLimit - 1; // Move back 4801 IDs
            id = id < 0 ? 0 : id;

            await SyncFromId(id);
        }

        private async Task SyncFromId(int startId)
        {
            await foreach (var (posts, afterId, maxId) in EnumeratePostsAsync(startId))
            {
                var approvedPosts = posts
                    .Where(p => !p.Flags.IsPending)
                    .ToList();

                await SubmitContentAsync(approvedPosts);
                await FlagRangeForDeletionAsync(afterId, maxId, posts);
            }
        }

        private async IAsyncEnumerable<(ICollection<Post> posts, int afterId, int maxId)> EnumeratePostsAsync(int startId)
        {
            var currentId = startId;
            while (true)
            {
                var posts = await LogEx.TimeAsync(async () =>
                {
                    return await HttpResiliency.RunAsync(() =>
                        _e621Client.GetPostsAsync(currentId, Position.After, E621Constants.PostsMaximumLimit));
                }, "Retrieving posts after ID {afterId}", currentId);

                if (!posts.Any())
                {
                    Log.Information("No posts were retrieved");
                    break;
                }

                var maxId = posts.Max(p => p.Id);
                yield return (posts, currentId, maxId);

                if (posts.Count != E621Constants.PostsMaximumLimit)
                    break;

                currentId = maxId;
            }
        }

        public override string GetViewLocation(Post src) => $"https://e621.net/posts/{src.Id}";

        public override IEnumerable<PutContentModel.FileModel> GetFiles(Post src)
        {
            static PutContentModel.FileModel PostFileToModel(PostImage postImage)
            {
                return new()
                {
                    Location = postImage.Location.OriginalString,
                    Format = FileFormatHelper.GetFileFormatFromExtension(postImage.FileExtension),
                    Width = postImage.Width,
                    Height = postImage.Height
                };
            }

            if (src.File == null)
                throw new InvalidOperationException("The e621 API returned a response in which the file attribute was null.");

            yield return PostFileToModel(src.File);

            if (src.Sample != null && src.Sample.Has)
                yield return PostFileToModel(src.Sample);

            if (src.Preview != null)
                yield return PostFileToModel(src.Preview);
        }

        public override IEnumerable<string> GetTags(Post src) => src.Tags.Valid;

        public override MediaTypeConstant GetMediaType(Post src)
        {
            var fileFormat = FileFormatHelper.GetFileFormatFromExtension(src.File.FileExtension);

            // We can't centralize this as the mappings below might not be valid for all platforms
            return fileFormat switch
            {
                FileFormatConstant.Png => MediaTypeConstant.Image,
                FileFormatConstant.Jpeg => MediaTypeConstant.Image,
                FileFormatConstant.WebM => MediaTypeConstant.Video,
                FileFormatConstant.Swf => MediaTypeConstant.Other,
                FileFormatConstant.Gif => MediaTypeConstant.AnimatedImage,
                FileFormatConstant.WebP => MediaTypeConstant.Image,
                _ => throw new ArgumentOutOfRangeException(nameof(src))
            };
        }

        public override int GetPriority(Post src) => src.Score.Total;

        public override IEnumerable<PutContentModel.CreditableEntityModel> GetCredits(Post src)
        {
            var models = src.Tags.Artist
                .Where(a => !a.Equals("unknown_artist", StringComparison.InvariantCultureIgnoreCase))
                .Where(a => !a.Equals("conditional_dnp", StringComparison.InvariantCultureIgnoreCase))
                .Select(a =>
                {
                    var index = a.LastIndexOf("_(artist)", StringComparison.OrdinalIgnoreCase);
                    var artist = index == -1 ? a : a[..index];
                    artist = artist.Replace('_', ' ');

                    return new PutContentModel.CreditableEntityModel
                    {
                        Id = a,
                        Name = artist,
                        Type = CreditableEntityType.Artist
                    };
                });

            return models;
        }

        public override string GetId(Post src) => src.Id.ToString();

        public override ContentRatingConstant GetRating(Post src)
        {
            return src.Rating switch
            {
                PostRating.Safe => ContentRatingConstant.Safe,
                PostRating.Questionable => ContentRatingConstant.Questionable,
                PostRating.Explicit => ContentRatingConstant.Explicit,
                _ => throw new ArgumentOutOfRangeException(nameof(src))
            };
        }

        public override string GetTitle(Post src) => null;

        public override string GetDescription(Post src) => src.Description;

        public override bool ShouldBeIndexed(Post src) => true;
    }
}

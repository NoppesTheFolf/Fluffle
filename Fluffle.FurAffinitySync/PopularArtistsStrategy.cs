using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static MoreLinq.Extensions.ExceptByExtension;
using static MoreLinq.Extensions.ForEachExtension;

namespace Noppes.Fluffle.FurAffinitySync
{
    public class PopularArtistsStrategy : FurAffinityContentProducerStrategy
    {
        private string _artist;
        private Queue<int> _submissionIds;

        public PopularArtistsStrategy(FluffleClient fluffleClient, FurAffinityClient faClient,
            FurAffinitySyncClientState state) : base(fluffleClient, faClient, state)
        {
        }

        public override async Task<FurAffinityContentProducerStateResult> NextAsync()
        {
            if (_submissionIds == null)
            {
                return await RefreshAsync() ? new FurAffinityContentProducerStateResult
                {
                    FaResult = null
                } : null;
            }

            if (!_submissionIds.TryDequeue(out var id))
            {
                _artist = null;
                _submissionIds = null;

                return null;
            }

            var getSubmissionResult = await LogEx.TimeAsync(async () =>
            {
                return await HttpResiliency.RunAsync(() => FaClient.GetSubmissionAsync(id));
            }, "Retrieving submission with ID {id} from artist {artist}", id, _artist);

            if (_submissionIds.Count == 0)
                State.ProcessedArtists.Add(_artist);

            return new FurAffinityContentProducerStateResult
            {
                FaResult = getSubmissionResult
            };
        }

        private async Task<bool> RefreshAsync()
        {
            var result = await FluffleClient.GetFaPopularArtistsAsync();
            var artists = result.ExceptBy(State.ProcessedArtists.Select(a => new FaPopularArtistModel
            {
                Artist = a
            }), r => r.Artist).OrderByDescending(r => r.Score).ToList();

            if (artists.Count == 0)
                return false;

            _artist = artists.First().Artist;
            _submissionIds = new Queue<int>();
            var gallery = await LogEx.TimeAsync(async () =>
            {
                return await HttpResiliency.RunAsync(() => FaClient.GetGalleryAsync(_artist));
            }, "Retrieved main gallery of artist {artist} from page {page}", _artist, 1);
            gallery.Result.SubmissionIds.ForEach(_submissionIds.Enqueue);

            var galleryParts = new List<(Func<int, Task<FaResult<FaGallery>>> getAsync, string messageTemplate, object[] args)>
            {
                (p => FaClient.GetGalleryAsync(_artist, p), "Retrieved main gallery of artist {artist} from page {page}", Array.Empty<object>()),
                (p => FaClient.GetScrapsAsync(_artist, p), "Retrieved scraps of artist {artist} from page {page}", Array.Empty<object>())
            };
            foreach (var folder in gallery.Result.Folders)
                galleryParts.Add((p => FaClient.GetGalleryAsync(_artist, p, folder), "Retrieved folder {folderName} of artist {artist} from page {page}", new object[] { folder.Title }));

            var page = gallery.Result.Page + 1;
            var hasNextPage = gallery.Result.HasNextPage;
            foreach (var galleryPart in galleryParts)
            {
                while (hasNextPage)
                {
                    gallery = await LogEx.TimeAsync(async () =>
                    {
                        return await HttpResiliency.RunAsync(() => galleryPart.getAsync(page));
                    }, galleryPart.messageTemplate, galleryPart.args.Concat(new object[] { _artist, page }).ToArray());
                    gallery.Result.SubmissionIds.ForEach(_submissionIds.Enqueue);

                    page++;
                    hasNextPage = gallery.Result.HasNextPage;
                }

                page = 1;
                hasNextPage = true;
            }

            return true;
        }
    }
}

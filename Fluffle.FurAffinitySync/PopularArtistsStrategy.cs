using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using Serilog;
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
        private Stack<int> _submissionIds;

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

            if (!_submissionIds.TryPop(out var id))
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
            var submissionIds = new HashSet<int>();
            var gallery = await LogEx.TimeAsync(async () =>
            {
                return await HttpResiliency.RunAsync(() => FaClient.GetGalleryAsync(_artist));
            }, "Retrieved main gallery of artist {artist} from page {page}", _artist, 1);

            if (gallery == null)
            {
                Log.Warning("The gallery of {artist} couldn't be retrieved because they have disabled their account.", _artist);
                State.ProcessedArtists.Add(_artist);
                _artist = null;
                return false;
            }

            gallery.Result.SubmissionIds.ForEach(id => submissionIds.Add(id));

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
                    gallery.Result.SubmissionIds.ForEach(id => submissionIds.Add(id));

                    page++;
                    hasNextPage = gallery.Result.HasNextPage;
                }

                page = 1;
                hasNextPage = true;
            }

            _submissionIds = new Stack<int>();
            submissionIds.ForEach(_submissionIds.Push);
            return true;
        }
    }
}

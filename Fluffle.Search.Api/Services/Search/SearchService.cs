using Microsoft.EntityFrameworkCore;
using Nitranium.PerceptualHashing;
using Nitranium.PerceptualHashing.Exceptions;
using Nitranium.PerceptualHashing.Utils;
using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.PerceptualHashing;
using Noppes.Fluffle.Search.Api.Models;
using Noppes.Fluffle.Search.Database;
using Noppes.Fluffle.Search.Database.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.Services
{
    public class SearchService : Service, ISearchService
    {
        private static readonly FileSignatureMatcher SafeFileSignatures =
            new(new JpegSignature(), new PngSignature(), new WebPSignature());

        private readonly PlatformSearchService _compareService;
        private readonly FluffleHash _hash;
        private readonly FluffleSearchContext _context;

        public SearchService(PlatformSearchService compareService, FluffleHash hash, FluffleSearchContext context)
        {
            _compareService = compareService;
            _hash = hash;
            _context = context;
        }

        public async Task<SR<SearchResultModel>> SearchAsync(SearchModel model)
        {
            var stopwatch = Stopwatch.StartNew();

            // Write the content embedded in the request to a temporary file
            using var temporaryFile = new TemporaryFile();
            await using (var temporaryFileStream = temporaryFile.OpenFileStream())
            {
                await model.Image.CopyToAsync(temporaryFileStream);
                temporaryFileStream.Position = 0;

                // Check if the uploaded file contains a signature of a web safe image type
                if (!SafeFileSignatures.TryMatch(temporaryFileStream, out _))
                    return new SR<SearchResultModel>(SearchError.UnsupportedFileType());
            }

            // We need to compute a more granular hash too as the 64-bit averaged hash is unable to
            // differentiate between alternate version. We do this asynchronously to computing the
            // simple hash and comparing that. We can assume that awaiting this task doesn't throw
            // any exceptions as this process is pretty much identical to computing the simple hash
            // (of which we handle the exceptions explicitly).
            var hash256Task = Task.Run(() =>
            {
                using var image = _hash.Size256.For(temporaryFile.Location);
                var redHash = image.ComputeHash(Channel.Red);
                var greenHash = image.ComputeHash(Channel.Green);
                var blueHash = image.ComputeHash(Channel.Blue);

                return (FluffleHash.ToInt64(redHash), FluffleHash.ToInt64(greenHash), FluffleHash.ToInt64(blueHash));
            });

            ulong hash;
            using (var image = _hash.Size64.For(temporaryFile.Location))
            {
                try
                {
                    var hashBytes = image.ComputeHash(Channel.Average);

                    hash = FluffleHash.ToUInt64(hashBytes);
                }
                catch (ConvertException)
                {
                    return new SR<SearchResultModel>(SearchError.CorruptImage());
                }
            }

            // TODO: Automatically determine degree of parallelism based on the hardware Fluffle is running on
            var searchResult = _compareService.Compare(hash, !model.IncludeNsfw, model.Limit * 2);

            var images = _context.Images.AsNoTracking()
                .IncludeThumbnails()
                .Include(i => i.ImageHash)
                .Include(i => i.Platform)
                .Include(i => i.Credits)
                .Where(i => searchResult.Images.Select(r => r.Id).Contains(i.Id) && !i.IsDeleted);

            var (red, green, blue) = await hash256Task;

            var models = images
                .AsEnumerable()
                .Select(r => new SearchResultModel.ImageModel
                {
                    Id = r.Id,
                    IsSfw = r.IsSfw,
                    Platform = r.Platform.Name,
                    ViewLocation = r.ViewLocation,
                    Score = CompareRgb(r.ImageHash, red, green, blue),
                    Thumbnail = ThumbnailModel(r.Thumbnail),
                    Credits = r.Credits.Select(c => new SearchResultModel.ImageModel.CreditModel
                    {
                        Name = c.Name,
                        Role = c.Type
                    })
                })
                .OrderByDescending(r => r.Score)
                .Take(model.Limit)
                .ToList();

            return new SR<SearchResultModel>(new SearchResultModel
            {
                Results = models,
                Stats = new SearchResultModel.StatsModel
                {
                    Count = searchResult.Count,
                    ElapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds,
                }
            });
        }

        private static double CompareRgb(ImageHash hashes, ReadOnlySpan<ulong> red, ReadOnlySpan<ulong> green, ReadOnlySpan<ulong> blue)
        {
            static int Compare(ReadOnlySpan<ulong> hash, ReadOnlySpan<ulong> otherHash)
            {
                ulong mismatchCount = 0;

                for (var i = 0; i < hash.Length; i++)
                    mismatchCount += Popcnt.X64.PopCount(hash[i] ^ otherHash[i]);

                return (int)mismatchCount;
            }

            // Compare all channels and select the one with the worst match
            var worstMismatchCount = new[]
            {
                Compare(red, FluffleHash.ToInt64(hashes.PhashRed256)),
                Compare(green, FluffleHash.ToInt64(hashes.PhashGreen256)),
                Compare(blue, FluffleHash.ToInt64(hashes.PhashBlue256)),
            }.Max();

            return (256 - worstMismatchCount) / (double)256 * 100;
        }

        private static SearchResultModel.ImageModel.ThumbnailModel ThumbnailModel(Thumbnail thumbnail)
        {
            return new()
            {
                Width = thumbnail.Width,
                CenterX = thumbnail.CenterX,
                Height = thumbnail.Height,
                CenterY = thumbnail.CenterY,
                Location = thumbnail.Location
            };
        }
    }
}

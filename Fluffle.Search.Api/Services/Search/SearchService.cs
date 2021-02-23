using Microsoft.EntityFrameworkCore;
using Nitranium.PerceptualHashing;
using Nitranium.PerceptualHashing.Exceptions;
using Nitranium.PerceptualHashing.Utils;
using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.PerceptualHashing;
using Noppes.Fluffle.Search.Api.Models;
using Noppes.Fluffle.Search.Database;
using Noppes.Fluffle.Search.Database.Models;
using System.Diagnostics;
using System.Linq;
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

            // Open temporary file
            using var temporaryFile = new TemporaryFile();
            await using (var temporaryFileStream = temporaryFile.OpenFileStream())
            {
                // Check if the uploaded file contains a signature of a web safe image type
                await model.Image.CopyToAsync(temporaryFileStream);
                temporaryFileStream.Position = 0;

                if (!SafeFileSignatures.TryMatch(temporaryFileStream, out _))
                    return new SR<SearchResultModel>(SearchError.UnsupportedFileType());
            }

            byte[] hashBytes;
            using (var image = _hash.Size64.For(temporaryFile.Location))
            {
                try
                {
                    hashBytes = image.ComputeHash(Channel.Average);
                }
                catch (ConvertException)
                {
                    return new SR<SearchResultModel>(SearchError.CorruptImage());
                }
            }
            var hash = FluffleHash.ToUInt64(hashBytes);

            // TODO: Automatically determine degree of parallelism
            var searchResult = _compareService.Compare(hash, !model.IncludeNsfw, model.Limit, 1);

            var mismatchDictionary = searchResult.Images
                .ToDictionary(r => r.Id, r => r.MismatchCount);

            var resultsInDb = _context.Images.AsNoTracking()
                .IncludeThumbnails()
                .Include(i => i.Platform)
                .Include(i => i.Credits)
                .Where(i => searchResult.Images.Select(r => r.Id).Contains(i.Id));

            var combined = resultsInDb
                .Select(r => new SearchResultModel.ImageModel
                {
                    Id = r.Id,
                    IsSfw = r.IsSfw,
                    Platform = r.Platform.Name,
                    ViewLocation = r.ViewLocation,
                    Score = (64 - mismatchDictionary[r.Id]) / (double)64 * 100,
                    Thumbnail = ThumbnailModel(r.Thumbnail),
                    Credits = r.Credits.Select(c => new SearchResultModel.ImageModel.CreditModel
                    {
                        Name = c.Name,
                        Role = c.Type
                    })
                })
                .AsEnumerable()
                .OrderByDescending(r => r.Score)
                .Take(model.Limit)
                .ToList();

            return new SR<SearchResultModel>(new SearchResultModel
            {
                Results = combined,
                Stats = new SearchResultModel.StatsModel
                {
                    Count = searchResult.Count,
                    ElapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds,
                }
            });
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

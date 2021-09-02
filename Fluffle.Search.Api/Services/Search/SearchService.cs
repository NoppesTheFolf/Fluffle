using Microsoft.EntityFrameworkCore;
using Nitranium.PerceptualHashing;
using Nitranium.PerceptualHashing.Exceptions;
using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.PerceptualHashing;
using Noppes.Fluffle.Search.Api.Models;
using Noppes.Fluffle.Search.Database;
using Noppes.Fluffle.Search.Database.Models;
using Noppes.Fluffle.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.Services
{
    public class SearchService : Service, ISearchService
    {
        private readonly PlatformSearchService _compareService;
        private readonly FluffleHash _hash;
        private readonly FluffleSearchContext _context;

        public SearchService(PlatformSearchService compareService, FluffleHash hash, FluffleSearchContext context)
        {
            _compareService = compareService;
            _hash = hash;
            _context = context;
        }

        public async Task<SR<SearchResultModel>> SearchAsync(string imageLocation, bool includeNsfw, int limit, ICollection<PlatformConstant> platforms, bool includeDebug, CheckpointStopwatchScope<SearchRequest> scope)
        {
            // We need to compute a more granular hash too as the 64-bit averaged hash is unable to
            // differentiate between alternate version. We do this asynchronously to computing the
            // simple hash and comparing that. We can assume that awaiting this task doesn't throw
            // any exceptions as this process is pretty much identical to computing the simple hash
            // (of which we handle the exceptions explicitly).
            scope.Next(t => t.Start256RgbComputation);
            var hash256Task = Task.Run(() =>
            {
                using var image = _hash.Size256.For(imageLocation);

                using var taskScope = scope.Stopwatch.ForCheckpoint(t => t.Compute256Red);
                var redHash = image.ComputeHash(Channel.Red);

                taskScope.Next(t => t.Compute256Green);
                var greenHash = image.ComputeHash(Channel.Green);

                taskScope.Next(t => t.Compute256Blue);
                var blueHash = image.ComputeHash(Channel.Blue);

                return (FluffleHash.ToInt64(redHash), FluffleHash.ToInt64(greenHash), FluffleHash.ToInt64(blueHash));
            });

            scope.Next(t => t.Compute64Average);
            ulong hash;
            using (var image = _hash.Size64.For(imageLocation))
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
            scope.Next(t => t.Compare64Average);
            var searchResult = _compareService.Compare(hash, !includeNsfw, limit * 2, platforms);
            var searchResultLookup = searchResult.Images.ToDictionary(i => i.Id);

            scope.Next(t => t.ComplementComparisonResults);
            var images = await _context.Images.AsNoTracking()
                .IncludeThumbnails()
                .Include(i => i.ImageHash)
                .Include(i => i.Platform)
                .Include(i => i.Credits)
                .Where(i => searchResult.Images.Select(r => r.Id).Contains(i.Id) && !i.IsDeleted)
                .ToListAsync();

            scope.Next(t => t.WaitFor256RgbComputation);
            var (red, green, blue) = await hash256Task;

            scope.Next(t => t.CreateAndRefineOutput);

            var models = images
                .Select(r =>
                {
                    var compareResult = CompareRgb(r.ImageHash, red, green, blue);

                    var model = new SearchResultModel.ImageModel
                    {
                        Id = r.Id,
                        IsSfw = r.IsSfw,
                        Platform = r.Platform.Name,
                        Location = r.ViewLocation,
                        Score = compareResult.Score,
                        Thumbnail = new SearchResultModel.ImageModel.ThumbnailModel
                        {
                            Id = r.Thumbnail.Id,
                            Width = r.Thumbnail.Width,
                            CenterX = r.Thumbnail.CenterX,
                            Height = r.Thumbnail.Height,
                            CenterY = r.Thumbnail.CenterY,
                            Location = r.Thumbnail.Location
                        },
                        Credits = r.Credits.OrderBy(c => c.Type).Select(c =>
                            new SearchResultModel.ImageModel.CreditModel
                            {
                                Id = c.Id,
                                Name = c.Name
                            }
                        ),
                        Stats = includeDebug ? new SearchResultModel.ImageModel.StatsModel
                        {
                            Average64 = (int)searchResultLookup[r.Id].MismatchCount,
                            Red256 = compareResult.Red,
                            Green256 = compareResult.Green,
                            Blue256 = compareResult.Blue
                        } : null
                    };

                    return model;
                })
                .OrderByDescending(r => r.Score)
                .Take(limit)
                .ToList();

            return new SR<SearchResultModel>(new SearchResultModel
            {
                Results = models,
                Stats = new SearchResultModel.StatsModel
                {
                    Count = searchResult.Count,
                    ElapsedMilliseconds = (int)scope.Stopwatch.ElapsedMilliseconds
                }
            });
        }

        public class CompareResult
        {
            public double Score { get; set; }

            public int Red { get; set; }

            public int Green { get; set; }

            public int Blue { get; set; }
        }

        private static CompareResult CompareRgb(ImageHash hashes, ReadOnlySpan<ulong> red, ReadOnlySpan<ulong> green, ReadOnlySpan<ulong> blue)
        {
            static int Compare(ReadOnlySpan<ulong> hash, ReadOnlySpan<ulong> otherHash)
            {
                ulong mismatchCount = 0;

                for (var i = 0; i < hash.Length; i++)
                    mismatchCount += Popcnt.X64.PopCount(hash[i] ^ otherHash[i]);

                return (int)mismatchCount;
            }

            // Compare all channels
            var result = new CompareResult
            {
                Red = Compare(red, FluffleHash.ToInt64(hashes.PhashRed256)),
                Green = Compare(green, FluffleHash.ToInt64(hashes.PhashGreen256)),
                Blue = Compare(blue, FluffleHash.ToInt64(hashes.PhashBlue256))
            };

            // Select the one with the worst match and base the score on that
            var worstMismatchCount = new[]
            {
                result.Red,
                result.Green,
                result.Blue
            }.Max();
            result.Score = (256 - worstMismatchCount) / (double)256;

            return result;
        }
    }
}

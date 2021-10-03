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
using System.Linq.Expressions;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using static MoreLinq.Extensions.DistinctByExtension;

namespace Noppes.Fluffle.Search.Api.Services
{
    public class SearchService : Service, ISearchService
    {
        private const int BestUnlikelyThreshold = 340;
        private const int VarianceAlternativeThreshold = 120;
        private const int WorstAlternativeThreshold = 55;
        private const int DistanceFromBestAlternativeThreshold = 35;

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
            Task<(ulong[] red, ulong[] green, ulong[] blue, ulong[] average)> StartHashCalculation(PerceptualHash perceptualHash, bool includeDebug, Expression<Func<SearchRequest, int?>> red, Expression<Func<SearchRequest, int?>> green, Expression<Func<SearchRequest, int?>> blue)
            {
                return Task.Run(() =>
                {
                    using var image = perceptualHash.For(imageLocation);

                    using var taskScope = scope.Stopwatch.ForCheckpoint(red);
                    var redHash = image.ComputeHash(Channel.Red);

                    taskScope.Next(green);
                    var greenHash = image.ComputeHash(Channel.Green);

                    taskScope.Next(blue);
                    var blueHash = image.ComputeHash(Channel.Blue);

                    var averageHash = includeDebug ? image.ComputeHash(Channel.Average) : Array.Empty<byte>();

                    return (FluffleHash.ToInt64(redHash), FluffleHash.ToInt64(greenHash), FluffleHash.ToInt64(blueHash), FluffleHash.ToInt64(averageHash));
                });
            }

            scope.Next(t => t.StartExpensiveRgbComputation);
            var hash1024Task = StartHashCalculation(_hash.Size1024, includeDebug, t => t.ComputeExpensiveRed, t => t.ComputeExpensiveGreen, t => t.ComputeExpensiveBlue);

            Task<(ulong[], ulong[], ulong[], ulong[])> hash256Task = null;
            if (includeDebug) hash256Task = StartHashCalculation(_hash.Size256, true, t => t.ComputeExpensiveRed, t => t.ComputeExpensiveGreen, t => t.ComputeExpensiveBlue);

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

            // Bug: The search service returns duplicate images
            searchResult.Images = searchResult.Images.DistinctBy(i => i.Id).ToList();

            var searchResultLookup = searchResult.Images.ToDictionary(i => i.Id);

            scope.Next(t => t.ComplementComparisonResults);
            var images = await _context.Images.AsNoTracking()
                .IncludeThumbnails()
                .Include(i => i.ImageHash)
                .Include(i => i.Platform)
                .Include(i => i.Credits)
                .Where(i => searchResult.Images.Select(r => r.Id).Contains(i.Id) && !i.IsDeleted)
                .ToListAsync();

            scope.Next(t => t.WaitForExpensiveRgbComputation);
            var hashes1024 = await hash1024Task;

            (ulong[], ulong[], ulong[], ulong[]) hashes256 = default;
            if (includeDebug) hashes256 = await hash256Task;

            scope.Next(t => t.CreateAndRefineOutput);
            var results = images
                .Select(r =>
                {
                    var compareResult1024 = CompareRgb((r.ImageHash.PhashRed1024, r.ImageHash.PhashGreen1024, r.ImageHash.PhashBlue1024, r.ImageHash.PhashAverage1024), hashes1024, includeDebug);

                    CompareResult compareResult256 = null;
                    if (includeDebug) compareResult256 = CompareRgb((r.ImageHash.PhashRed256, r.ImageHash.PhashGreen256, r.ImageHash.PhashBlue256, r.ImageHash.PhashAverage256), hashes256, true);

                    var model = new SearchResultModel.ImageModel
                    {
                        Id = r.Id,
                        IsSfw = r.IsSfw,
                        Platform = r.Platform.Name,
                        Location = r.ViewLocation,
                        Score = compareResult1024.Score,
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
                            Red256 = compareResult256.Red,
                            Green256 = compareResult256.Green,
                            Blue256 = compareResult256.Blue,
                            Average256 = compareResult256.Average,
                            Red1024 = compareResult1024.Red,
                            Green1024 = compareResult1024.Green,
                            Blue1024 = compareResult1024.Blue,
                            Average1024 = compareResult1024.Average
                        } : null
                    };

                    return new
                    {
                        CompareResult = compareResult1024,
                        Model = model
                    };
                })
                .OrderBy(x => x.CompareResult.Mean)
                .Take(limit)
                .ToList();

            var bestMatch = results[0].CompareResult;
            foreach (var result in results)
                result.CompareResult.DistanceFromBest = result.CompareResult.Mean - bestMatch.Mean;

            foreach (var group in results.GroupBy(r => r.Model.Platform))
            {
                var bestInGroup = group.OrderBy(r => r.CompareResult.Mean).First();
                bestInGroup.CompareResult.IsBestOnPlatform = true;
            }

            ResultMatch Predict(CompareResult compareResult)
            {
                if (compareResult.Best > BestUnlikelyThreshold)
                    return ResultMatch.Unlikely;

                if (compareResult.Variance > VarianceAlternativeThreshold)
                    return ResultMatch.Alternative;

                if (compareResult.Worst > WorstAlternativeThreshold)
                    return ResultMatch.Alternative;

                if (compareResult.DistanceFromBest > DistanceFromBestAlternativeThreshold)
                    return ResultMatch.Alternative;

                return compareResult.IsBestOnPlatform ? ResultMatch.Exact : ResultMatch.TossUp;
            }

            foreach (var result in results)
                result.Model.Match = Predict(result.CompareResult);

            var models = results
                .Select(r => r.Model)
                .OrderByDescending(m => m.Score);

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

            public int Worst { get; set; }

            public int Best { get; set; }

            public double Mean { get; set; }

            public double DistanceFromBest { get; set; }

            public bool IsBestOnPlatform { get; set; }

            public double Variance { get; set; }

            public int Average { get; set; }
        }

        private static CompareResult CompareRgb((byte[] red, byte[] green, byte[] blue, byte[] average) hashesOne, (ulong[] red, ulong[] green, ulong[] blue, ulong[] average) hashesTwo, bool includeDebug)
        {
            var bits = sizeof(ulong) * hashesTwo.red.Length * 8;

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
                Red = Compare(hashesTwo.red, FluffleHash.ToInt64(hashesOne.red)),
                Green = Compare(hashesTwo.green, FluffleHash.ToInt64(hashesOne.green)),
                Blue = Compare(hashesTwo.blue, FluffleHash.ToInt64(hashesOne.blue)),
                Average = includeDebug ? Compare(hashesTwo.average, FluffleHash.ToInt64(hashesOne.average)) : -1,
            };

            // Calculate the ones with the worst and best match and base the score on the worst one
            var values = new[]
            {
                result.Red,
                result.Green,
                result.Blue
            };
            result.Worst = values.Max();
            result.Best = values.Min();

            result.Mean = values.Average();
            result.Variance = values.Select(x => Math.Pow(x - result.Mean, 2)).Average();
            result.Score = (bits - result.Worst) / (double)bits;

            return result;
        }
    }
}

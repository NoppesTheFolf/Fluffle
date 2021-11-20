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
using System.Collections.Immutable;
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

        private readonly ICompareClient _compareClient;
        private readonly FluffleHash _hash;
        private readonly FluffleSearchContext _context;

        public SearchService(ICompareClient compareClient, FluffleHash hash, FluffleSearchContext context)
        {
            _compareClient = compareClient;
            _hash = hash;
            _context = context;
        }

        public async Task<SR<SearchResultModel>> SearchAsync(string imageLocation, bool includeNsfw, int limit, ImmutableHashSet<PlatformConstant> platforms, bool includeDebug, CheckpointStopwatchScope<SearchRequest> scope)
        {
            // We need to compute a more granular hash too as the 64-bit averaged hash is unable to
            // differentiate between alternate version. We first calculate the complex hashes because
            // the libvips imaging provider can optimize itself when doing this.
            (ulong[] red, ulong[] green, ulong[] blue, ulong[] average) CalculateHashes(PerceptualHashImage perceptualHashImage, Expression<Func<SearchRequest, int?>> red, Expression<Func<SearchRequest, int?>> green, Expression<Func<SearchRequest, int?>> blue)
            {
                scope.Next(red);
                var redHash = perceptualHashImage.ComputeHash(Channel.Red);

                scope.Next(green);
                var greenHash = perceptualHashImage.ComputeHash(Channel.Green);

                scope.Next(blue);
                var blueHash = perceptualHashImage.ComputeHash(Channel.Blue);

                var averageHash = includeDebug ? perceptualHashImage.ComputeHash(Channel.Average) : Array.Empty<byte>();

                return (FluffleHash.ToInt64(redHash), FluffleHash.ToInt64(greenHash), FluffleHash.ToInt64(blueHash), FluffleHash.ToInt64(averageHash));
            }

            scope.Next(t => t.StartExpensiveRgbComputation);
            var hash = _hash.Create(128);
            using var hasher = hash.For(imageLocation);
            (ulong[] red, ulong[] green, ulong[] blue, ulong[] average) hashes1024 = default;
            try
            {
                hashes1024 = CalculateHashes(hasher, t => t.ComputeExpensiveRed, t => t.ComputeExpensiveGreen, t => t.ComputeExpensiveBlue);
            }
            catch (ConvertException)
            {
                return new SR<SearchResultModel>(SearchError.CorruptImage());
            }

            hash.Size = 32;
            (ulong[], ulong[], ulong[], ulong[]) hashes256 = default;
            if (includeDebug) hashes256 = CalculateHashes(hasher, t => t.ComputeExpensiveRed, t => t.ComputeExpensiveGreen, t => t.ComputeExpensiveBlue);

            scope.Next(t => t.Compute64Average);
            hash.Size = 8;
            var hash64 = FluffleHash.ToUInt64(hasher.ComputeHash(Channel.Average));

            scope.Next(t => t.Compare64Average);
            var searchResult = await _compareClient.CompareAsync(hash64, includeNsfw, limit);

            // Bug: The search service returns duplicate images. Update: might not anymore, who knows
            var searchResultImages = searchResult
                .SelectMany(x => x.Value.Images)
                .DistinctBy(i => i.Id)
                .ToList();

            var searchResultLookup = searchResultImages.ToDictionary(i => i.Id);

            scope.Next(t => t.ComplementComparisonResults);
            var images = await _context.Images.AsNoTracking()
                .IncludeThumbnails()
                .Include(i => i.ImageHash)
                .Include(i => i.Credits)
                .Where(i => searchResultImages.Select(r => r.Id).Contains(i.Id) && !i.IsDeleted)
                .ToListAsync();

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
                        Platform = (PlatformConstant)r.PlatformId,
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
                .Where(r => platforms.Contains(r.Model.Platform))
                .Select(r => r.Model)
                .OrderByDescending(m => m.Score)
                .Take(limit);

            return new SR<SearchResultModel>(new SearchResultModel
            {
                Results = models,
                Stats = new SearchResultModel.StatsModel
                {
                    Count = searchResult.Where(kv => platforms.Contains((PlatformConstant)kv.Key)).Sum(kv => kv.Value.Count),
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

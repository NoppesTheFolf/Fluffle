using Microsoft.EntityFrameworkCore;
using Nitranium.PerceptualHashing;
using Nitranium.PerceptualHashing.Exceptions;
using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.PerceptualHashing;
using Noppes.Fluffle.Search.Api.Models;
using Noppes.Fluffle.Search.Database.Models;
using Noppes.Fluffle.Utils;
using System;
using System.Collections.Generic;
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
        private const int Mismatch256Threshold = 72;

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
            (ulong[] red, ulong[] green, ulong[] blue, ulong[] average) CalculateHashes(
                PerceptualHashImage perceptualHashImage,
                bool red, Expression<Func<SearchRequest, int?>> timeRed,
                bool green, Expression<Func<SearchRequest, int?>> timeGreen,
                bool blue, Expression<Func<SearchRequest, int?>> timeBlue,
                bool average)
            {
                scope.Next(timeRed);
                var redHash = red ? perceptualHashImage.ComputeHash(Channel.Red) : Array.Empty<byte>();

                scope.Next(timeGreen);
                var greenHash = green ? perceptualHashImage.ComputeHash(Channel.Green) : Array.Empty<byte>();

                scope.Next(timeBlue);
                var blueHash = blue ? perceptualHashImage.ComputeHash(Channel.Blue) : Array.Empty<byte>();

                var averageHash = average ? perceptualHashImage.ComputeHash(Channel.Average) : Array.Empty<byte>();

                return (FluffleHash.ToInt64(redHash), FluffleHash.ToInt64(greenHash), FluffleHash.ToInt64(blueHash), FluffleHash.ToInt64(averageHash));
            }

            scope.Next(t => t.StartExpensiveRgbComputation);
            var hash = _hash.Create(128);
            using var hasher = hash.For(imageLocation);
            (ulong[] red, ulong[] green, ulong[] blue, ulong[] average) hashes1024 = default;
            try
            {
                hashes1024 = CalculateHashes(hasher, true, t => t.ComputeExpensiveRed, true, t => t.ComputeExpensiveGreen, true, t => t.ComputeExpensiveBlue, includeDebug);
            }
            catch (ConvertException)
            {
                return new SR<SearchResultModel>(SearchError.CorruptImage());
            }

            hash.Size = 32;
            var hashes256 = CalculateHashes(hasher, includeDebug, t => t.ComputeExpensiveRed, includeDebug, t => t.ComputeExpensiveGreen, includeDebug, t => t.ComputeExpensiveBlue, true);

            scope.Next(t => t.Compute64Average);
            hash.Size = 8;
            var hash64 = FluffleHash.ToUInt64(hasher.ComputeHash(Channel.Average));

            scope.Next(t => t.Compare64Average);
            var searchResult = await _compareClient.CompareAsync(hash64, hashes256.average, includeNsfw, limit);

            // Bug: The search service returns duplicate images. Update: might not anymore, who knows
            var searchResultImages = searchResult
                .SelectMany(x => x.Value.Images.Select(i => (platform: (PlatformConstant)x.Key, image: i)))
                .DistinctBy(x => x.image.Id)
                .Select(x =>
                {
                    var thresholdWeight = x.image.MismatchCount < Mismatch256Threshold ? 2 : 0;
                    var platformWeight = platforms.Contains(x.platform) ? 1 : 0;

                    return new
                    {
                        Image = x.image,
                        Priority = thresholdWeight + platformWeight
                    };
                })
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.Image.MismatchCount)
                .Select(x => x.Image)
                .Take(limit + limit / 2) // The comparison service may be a bit behind on deleted images, so it is good to take a bit more than needed
                .ToList();

            var searchResultLookup = searchResultImages.ToDictionary(i => i.Id);

            scope.Next(t => t.ComplementComparisonResults);
            var images = await _context.DenormalizedImages.AsNoTracking()
                .Where(i => searchResultImages.Select(r => r.Id).Contains(i.Id) && !i.IsDeleted)
                .ToListAsync();
            var imagesLookup = images.ToDictionary(i => i.Id);

            scope.Next(t => t.CreateAndRefineOutput);
            var results = images
                .Select(r =>
                {
                    var compareResult1024 = CompareRgb((r.PhashRed1024, r.PhashGreen1024, r.PhashBlue1024, r.PhashAverage1024), hashes1024, includeDebug);

                    CompareResult compareResult256 = null;
                    if (includeDebug) compareResult256 = CompareRgb((r.PhashRed256, r.PhashGreen256, r.PhashBlue256, r.PhashAverage256), hashes256, true);

                    var model = new SearchResultModel.ImageModel
                    {
                        Id = r.Id,
                        IsSfw = r.IsSfw,
                        Platform = (PlatformConstant)r.PlatformId,
                        Location = r.Location,
                        Score = compareResult1024.Score,
                        Thumbnail = new SearchResultModel.ImageModel.ThumbnailModel
                        {
                            Width = r.ThumbnailWidth,
                            CenterX = r.ThumbnailCenterX,
                            Height = r.ThumbnailHeight,
                            CenterY = r.ThumbnailCenterY,
                            Location = r.ThumbnailLocation
                        },
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
                .Take(limit)
                .ToList();

            var creditableEntityIds = models.SelectMany(m => imagesLookup[m.Id].Credits);
            var creditsLookup = await _context.CreditableEntities.AsNoTracking()
                .Where(ce => creditableEntityIds.Contains(ce.Id))
                .ToDictionaryAsync(c => c.Id);

            IEnumerable<CreditableEntity> GetCredits(IEnumerable<int> creditIds)
            {
                foreach (var creditId in creditIds)
                    if (creditsLookup.TryGetValue(creditId, out var creditableEntity))
                        yield return creditableEntity;
            }

            foreach (var model in models)
            {
                model.Credits = GetCredits(imagesLookup[model.Id].Credits)
                    .OrderBy(c => c.Type)
                    .Select(c => new SearchResultModel.ImageModel.CreditModel
                    {
                        Id = c.Id,
                        Name = c.Name
                    });
            }

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

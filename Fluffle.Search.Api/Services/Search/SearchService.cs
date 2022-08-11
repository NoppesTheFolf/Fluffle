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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.Services
{
    public class SearchService : Service, ISearchService
    {
        public static readonly Regex WeasylRegex = new("\\/submission\\/([0-9]*)", RegexOptions.Compiled);
        public static readonly Regex WwwRegex = new("https?:\\/\\/www\\.", RegexOptions.Compiled);

        private const int Mismatch256Threshold = 72;

        private const int BestUnlikelyThreshold = 340;
        private const int DistanceFromBestTossUpThreshold = 20;
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

        public async Task<SR<SearchResultModel>> SearchAsync(string imageLocation, bool includeNsfw, int limit, ImmutableHashSet<PlatformConstant> platforms, bool includeDebug, CheckpointStopwatchScope<SearchRequestV2> scope)
        {
            // We need to compute a more granular hash too as the 64-bit averaged hash is unable to
            // differentiate between alternate version. We first calculate the complex hashes because
            // the libvips imaging provider can optimize itself when doing this.
            HashCollection CalculateHashes(
                PerceptualHashImage perceptualHashImage,
                bool red, Expression<Func<SearchRequestV2, int?>> timeRed,
                bool green, Expression<Func<SearchRequestV2, int?>> timeGreen,
                bool blue, Expression<Func<SearchRequestV2, int?>> timeBlue,
                bool average, Expression<Func<SearchRequestV2, int?>> timeAverage)
            {
                if (red)
                    scope.Next(timeRed);
                var redHash = red ? perceptualHashImage.ComputeHash(Channel.Red) : Array.Empty<byte>();

                if (green)
                    scope.Next(timeGreen);
                var greenHash = green ? perceptualHashImage.ComputeHash(Channel.Green) : Array.Empty<byte>();

                if (blue)
                    scope.Next(timeBlue);
                var blueHash = blue ? perceptualHashImage.ComputeHash(Channel.Blue) : Array.Empty<byte>();

                if (average)
                    scope.Next(timeAverage);
                var averageHash = average ? perceptualHashImage.ComputeHash(Channel.Average) : Array.Empty<byte>();

                return new HashCollection(FluffleHash.ToInt64(redHash), FluffleHash.ToInt64(greenHash), FluffleHash.ToInt64(blueHash), FluffleHash.ToInt64(averageHash));
            }

            var hash = _hash.Create(128);
            using var hasher = hash.For(imageLocation);
            HashCollection hashes1024;
            try
            {
                hashes1024 = CalculateHashes(hasher, true, t => t.Compute1024Red, true, t => t.Compute1024Green, true, t => t.Compute1024Blue, includeDebug, t => t.Compute1024Average);
            }
            catch (ConvertException)
            {
                return new SR<SearchResultModel>(SearchError.CorruptImage());
            }

            hash.Size = 32;
            var hashes256 = CalculateHashes(hasher, includeDebug, t => t.Compute256Red, includeDebug, t => t.Compute256Green, includeDebug, t => t.Compute256Blue, true, t => t.Compute256Average);

            scope.Next(t => t.Compute64Average);
            hash.Size = 8;
            var hash64 = FluffleHash.ToUInt64(hasher.ComputeHash(Channel.Average));

            return await SearchAsync(hash64, hashes256, hashes1024, includeNsfw, limit, platforms, includeDebug, scope);
        }

        public Task<SR<SearchResultModel>> SearchAsync(ImageHash hash, bool includeNsfw, int limit, ImmutableHashSet<PlatformConstant> platforms, bool includeDebug, CheckpointStopwatchScope<SearchRequestV2> scope)
        {
            var hash64 = FluffleHash.ToUInt64(hash.PhashAverage64);
            var hashes256 = new HashCollection(FluffleHash.ToInt64(hash.PhashRed256), FluffleHash.ToInt64(hash.PhashGreen256), FluffleHash.ToInt64(hash.PhashBlue256), FluffleHash.ToInt64(hash.PhashAverage256));
            var hashes1024 = new HashCollection(FluffleHash.ToInt64(hash.PhashRed1024), FluffleHash.ToInt64(hash.PhashGreen1024), FluffleHash.ToInt64(hash.PhashBlue1024), FluffleHash.ToInt64(hash.PhashAverage1024));

            return SearchAsync(hash64, hashes256, hashes1024, includeNsfw, limit, platforms, includeDebug, scope);
        }

        public class SearchResult
        {
            public CompareResult CompareResult { get; set; }

            public SearchResultModel.ImageModel Model { get; set; }

            public ICollection<CreditableEntity> CreditableEntities { get; set; }
        }

        public async Task<SR<SearchResultModel>> SearchAsync(ulong hash64, HashCollection hashes256, HashCollection hashes1024, bool includeNsfw, int limit, ImmutableHashSet<PlatformConstant> platforms, bool includeDebug, CheckpointStopwatchScope<SearchRequestV2> scope)
        {
            scope.Next(t => t.CompareCoarse);
            var searchResult = await _compareClient.CompareAsync(hash64, hashes256.Average, includeNsfw, limit);

            // Bug: The search service returns duplicate images. Update: might not anymore, who knows
            scope.Next(t => t.ReduceCoarseResults);
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

            scope.Next(t => t.RetrieveImageInfo);
            var images = await _context.DenormalizedImages.AsNoTracking()
                .Where(i => searchResultImages.Select(r => r.Id).Contains(i.Id) && !i.IsDeleted)
                .ToListAsync();
            var imagesLookup = images.ToDictionary(i => i.Id);

            scope.Next(t => t.CompareGranular);
            var searchResults = images
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

                    return new SearchResult
                    {
                        CompareResult = compareResult1024,
                        Model = model
                    };
                })
                .OrderBy(sr => sr.CompareResult.Mean)
                .ToList();

            var bestMatch = searchResults[0].CompareResult;
            foreach (var result in searchResults)
                result.CompareResult.DistanceFromBest = result.CompareResult.Mean - bestMatch.Mean;

            ResultMatch Predict(CompareResult compareResult)
            {
                if (compareResult.Best > BestUnlikelyThreshold)
                    return ResultMatch.Unlikely;

                if (compareResult.DistanceFromBest > DistanceFromBestAlternativeThreshold)
                    return ResultMatch.Alternative;

                if (compareResult.DistanceFromBest > DistanceFromBestTossUpThreshold)
                    return ResultMatch.TossUp;

                return ResultMatch.Exact;
            }

            foreach (var result in searchResults)
                result.Model.Match = Predict(result.CompareResult);

            scope.Next(t => t.ReduceGranularResults);
            searchResults = searchResults
                .Where(sr => platforms.Contains(sr.Model.Platform))
                .OrderByDescending(sr => sr.Model.Score)
                .Take(limit)
                .ToList();

            // Clean up the view location
            scope.Next(t => t.CleanViewLocation);
            foreach (var model in searchResults.Select(x => x.Model))
            {
                if (model.Platform == PlatformConstant.Weasyl)
                {
                    var match = WeasylRegex.Match(model.Location);
                    if (match.Success)
                        model.Location = $"https://weasyl.com/submission/{match.Groups[1].Value}";

                    continue;
                }

                if (model.Platform == PlatformConstant.FurAffinity)
                {
                    var match = WwwRegex.Match(model.Location);
                    if (match.Success)
                        model.Location = $"https://{model.Location[match.Length..]}";

                    continue;
                }
            }

            scope.Next(t => t.RetrieveCreditableEntities);
            var creditableEntityIds = searchResults.SelectMany(sr => imagesLookup[sr.Model.Id].Credits);
            var creditsLookup = await _context.CreditableEntities.AsNoTracking()
                .Where(ce => creditableEntityIds.Contains(ce.Id))
                .ToDictionaryAsync(c => c.Id);

            scope.Next(t => t.AppendCreditableEntities);
            IEnumerable<CreditableEntity> GetCredits(IEnumerable<int> creditIds)
            {
                foreach (var creditId in creditIds)
                    if (creditsLookup.TryGetValue(creditId, out var creditableEntity))
                        yield return creditableEntity;
            }

            foreach (var result in searchResults)
            {
                result.CreditableEntities = GetCredits(imagesLookup[result.Model.Id].Credits).ToList();

                result.Model.Credits = result.CreditableEntities
                    .OrderBy(c => c.Type)
                    .Select(c => new SearchResultModel.ImageModel.CreditModel
                    {
                        Id = c.Id,
                        Name = c.Name
                    }).ToList();
            }

            scope.Next(t => t.DetermineFinalOrder);
            foreach (var group in searchResults.Where(sr => sr.Model.Match is ResultMatch.Exact or ResultMatch.TossUp).GroupBy(sr => sr.Model.Platform))
            {
                var orderedGroup = group
                    .OrderByDescending(sr => sr.CreditableEntities
                        .OrderByDescending(ce => ce.Priority ?? int.MinValue)
                        .Select(ce => ce.Priority)
                        .FirstOrDefault() ?? int.MinValue)
                    .ThenBy(sr => sr.CompareResult.Mean)
                    .ToList();

                for (var i = 0; i < orderedGroup.Count; i++)
                    orderedGroup[i].Model.Match = i == 0 ? ResultMatch.Exact : ResultMatch.TossUp;
            }

            return new SR<SearchResultModel>(new SearchResultModel
            {
                Results = searchResults.Select(x => x.Model)
                    .OrderByDescending(x => x.Match)
                    .ThenByDescending(x => x.Score)
                    .ToList(),
                Stats = new SearchResultModel.StatsModel
                {
                    Count = searchResult
                        .Where(kv => platforms.Contains((PlatformConstant)kv.Key))
                        .Sum(kv => kv.Value.Count)
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

            public double Variance { get; set; }

            public int Average { get; set; }
        }

        private static CompareResult CompareRgb((byte[] red, byte[] green, byte[] blue, byte[] average) hashesOne, HashCollection hashesTwo, bool includeDebug)
        {
            var bits = sizeof(ulong) * hashesTwo.Red.Length * 8;

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
                Red = Compare(hashesTwo.Red, FluffleHash.ToInt64(hashesOne.red)),
                Green = Compare(hashesTwo.Green, FluffleHash.ToInt64(hashesOne.green)),
                Blue = Compare(hashesTwo.Blue, FluffleHash.ToInt64(hashesOne.blue)),
                Average = includeDebug ? Compare(hashesTwo.Average, FluffleHash.ToInt64(hashesOne.average)) : -1,
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

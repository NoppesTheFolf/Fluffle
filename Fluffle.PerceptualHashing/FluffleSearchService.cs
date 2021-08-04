using Nito.AsyncEx;
using Noppes.Fluffle.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

namespace Noppes.Fluffle.PerceptualHashing
{
    /// <summary>
    /// The result produced the <see cref="FluffleSearchService{TPlatform}.Compare"/>
    /// </summary>
    public class SearchResult
    {
        public ICollection<ComparedImage> Images { get; set; }

        public int Count { get; set; }
    }

    /// <summary>
    /// Internally used class to handle multi-threaded searching.
    /// </summary>
    internal class CompareResult : IDisposable
    {
        public DiscardingCollection<ComparedImage>[] Images { get; set; }

        public CompareResult(int size)
        {
            Images = new DiscardingCollection<ComparedImage>[size];
        }

        public void Dispose()
        {
            foreach (var result in Images)
                result.Dispose();
        }
    }

    // TODO: This class is doing more than it should be doing
    public class FluffleSearchService<TPlatform>
    {
        private const int Threshold = 20;

        private readonly AsyncReaderWriterLock _mutex;
        private readonly IDictionary<TPlatform, FluffleRatedHashCollection> _hashesLookup;

        public FluffleSearchService()
        {
            _mutex = new AsyncReaderWriterLock();
            _hashesLookup = new Dictionary<TPlatform, FluffleRatedHashCollection>();
        }

        public void Add(TPlatform platform, HashedImage image, bool isSfw)
        {
            using var _ = _mutex.WriterLock();

            if (!_hashesLookup.TryGetValue(platform, out var hashes))
            {
                hashes = new FluffleRatedHashCollection();
                _hashesLookup.Add(platform, hashes);
            }

            hashes.Add(image, isSfw);
        }

        public void Remove(TPlatform platform, int imageId)
        {
            using var _ = _mutex.WriterLock();

            if (!_hashesLookup.TryGetValue(platform, out var hashes))
                return;

            hashes.Remove(imageId);
        }

        public SearchResult Compare(ulong hash, bool sfwOnly, int limit, ICollection<TPlatform> platforms, int degreeOfParallelism = 1)
        {
            using var _ = _mutex.ReaderLock();

            if (degreeOfParallelism < 1)
                throw new ArgumentOutOfRangeException(nameof(degreeOfParallelism));

            var hashes = Hashes(sfwOnly, platforms);
            var memories = new MemoryCollection<HashedImage>(hashes);

            using var compareResult = new CompareResult(degreeOfParallelism);

            for (var i = 0; i < compareResult.Images.Length; i++)
                compareResult.Images[i] = new DiscardingCollection<ComparedImage>(65, 40);

            if (degreeOfParallelism == 1)
            {
                Compare(compareResult.Images[0], memories.Batch(1).First(), hash);
                return HandleComparisonResult(compareResult, limit, memories.Length);
            }

            var buckets = memories.Batch(degreeOfParallelism);
            var tasks = new Task[degreeOfParallelism];

            var count = 0;
            foreach (var bucket in buckets)
            {
                var countCopy = count;
                tasks[countCopy] = Task.Run(() => Compare(compareResult.Images[countCopy], bucket, hash));

                count++;
            }

            Task.WaitAll(tasks);

            return HandleComparisonResult(compareResult, limit, memories.Length);
        }

        private SearchResult HandleComparisonResult(CompareResult compareResult, int limit, int count)
        {
            var images = compareResult.Images
                .SelectMany(i => i.Take(limit))
                .ToList();

            return new SearchResult
            {
                Images = images,
                Count = count
            };
        }

        private IEnumerable<Memory<HashedImage>> Hashes(bool sfwOnly, IEnumerable<TPlatform> platforms)
        {
            foreach (var platform in platforms)
                if (_hashesLookup.TryGetValue(platform, out var hashes))
                    foreach (var memory in hashes.AsMemories(sfwOnly))
                        yield return memory;
        }

        private static void Compare(DiscardingCollection<ComparedImage> comparedImages, IEnumerable<Memory<HashedImage>> hashMemories, ulong otherHash)
        {
            foreach (var memory in hashMemories)
            {
                foreach (var image in memory.Span)
                {
                    // XOR the hash and use the POPCNT instruction to count the number of bits set
                    var mismatchCount = Popcnt.X64.PopCount(image.Hash ^ otherHash);

                    // Only try adding the hash if the mismatch count is below a certain threshold.
                    // Adding all of the compared hashes would take a huge toll on performance
                    if (mismatchCount <= Threshold)
                        comparedImages.Add(mismatchCount, new ComparedImage(image.Id, mismatchCount));
                }
            }
        }
    }
}

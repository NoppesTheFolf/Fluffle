using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using Noppes.Fluffle.Search.Business.Similarity;

namespace Noppes.Fluffle.Search.Benchmarks;

[MemoryDiagnoser]
public class SimilarityServiceNearestNeighborsBenchmark
{
    private const string DumpLocation = "C:\\FluffleSimilarityDataDump"; // TODO: Should be made configurable

    private SimilarityService _service = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        var serializer = new FileSystemSimilarityDataSerializer(DumpLocation, new NullLogger<FileSystemSimilarityDataSerializer>());
        _service = new SimilarityService(serializer, null!, new NullLogger<SimilarityService>());

        var restoredDump = await _service.TryRestoreDumpAsync();
        if (restoredDump == null)
            throw new InvalidOperationException("No dump was restored.");
    }

    [Benchmark]
    public IDictionary<int, SimilarityResult> NearestNeighbors()
    {
        // Uses hash of https://e621.net/posts/546281
        return _service.NearestNeighbors(7750049072314344851ul, new[]
        {
            13176374055309646235ul,
            5506568146837054353ul,
            2962781332219246054ul,
            16819401342261807850ul
        }, true, 32);
    }
}

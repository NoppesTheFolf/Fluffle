using BenchmarkDotNet.Running;

namespace Noppes.Fluffle.Search.Business.Benchmarks;

internal class Program
{
    private static void Main()
    {
        BenchmarkRunner.Run<SimilarityServiceNearestNeighborsBenchmark>();
    }
}

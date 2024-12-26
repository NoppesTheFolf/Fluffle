using BenchmarkDotNet.Running;

namespace Noppes.Fluffle.Search.Benchmarks;

internal class Program
{
    private static void Main()
    {
        BenchmarkRunner.Run<ImageHashesDeserializerBenchmark>();
    }
}

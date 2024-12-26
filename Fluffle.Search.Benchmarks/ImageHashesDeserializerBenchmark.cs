using BenchmarkDotNet.Attributes;
using Noppes.Fluffle.Search.Database;
using System.IO.Compression;

namespace Noppes.Fluffle.Search.Benchmarks;

[MemoryDiagnoser]
public class ImageHashesDeserializerBenchmark
{
    private byte[] _randomBytes = null!;

    [IterationSetup]
    public void GlobalSetup()
    {
        Span<byte> randomBytes = stackalloc byte[64 / 8 + 256 / 8 * 4 + 1024 / 8 * 4];
        Random.Shared.NextBytes(randomBytes);

        using var compressedStream = new MemoryStream();
        using (var brotliStream = new BrotliStream(compressedStream, CompressionLevel.SmallestSize))
        {
            brotliStream.Write(randomBytes);
        }

        _randomBytes = compressedStream.ToArray();
    }

    [Benchmark]
    public ulong NearestNeighbors()
    {
        var result = ulong.MaxValue;
        for (var i = 0; i < 250_000; i++)
        {
            var imageHashes = ImageHashesDeserializer.Deserialize(_randomBytes);
            result ^= imageHashes.PhashAverage64;
        }

        return result;
    }
}

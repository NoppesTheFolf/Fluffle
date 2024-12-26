using Noppes.Fluffle.Search.Domain;
using Noppes.Fluffle.Utils;
using System;
using System.IO;
using System.IO.Compression;

namespace Noppes.Fluffle.Search.Database;

public class ImageHashesDeserializer
{
    public static ImageHashes Deserialize(byte[] compressedImageHashes)
    {
        using var compressedStream = new MemoryStream(compressedImageHashes);
        using var decompressedStream = new BrotliStream(compressedStream, CompressionMode.Decompress);

        const int size64 = 64 / 8;
        const int size256 = 256 / 8;
        const int size1024 = 1024 / 8;

        var imageHashes = new ImageHashes
        {
            PhashAverage64 = ReadInt64(decompressedStream, size64),
            PhashRed256 = ReadInt64Array(decompressedStream, size256),
            PhashGreen256 = ReadInt64Array(decompressedStream, size256),
            PhashBlue256 = ReadInt64Array(decompressedStream, size256),
            PhashAverage256 = ReadInt64Array(decompressedStream, size256),
            PhashRed1024 = ReadInt64Array(decompressedStream, size1024),
            PhashGreen1024 = ReadInt64Array(decompressedStream, size1024),
            PhashBlue1024 = ReadInt64Array(decompressedStream, size1024),
            PhashAverage1024 = ReadInt64Array(decompressedStream, size1024)
        };
        return imageHashes;
    }

    private static ulong ReadInt64(Stream stream, int nBytes)
    {
        Span<byte> buffer = stackalloc byte[nBytes];
        stream.ReadExactly(buffer);

        return ByteConvert.ToUInt64(buffer);
    }

    private static ulong[] ReadInt64Array(Stream stream, int nBytes)
    {
        Span<byte> buffer = stackalloc byte[nBytes];
        stream.ReadExactly(buffer);

        return ByteConvert.ToInt64(buffer);
    }
}

using System.Buffers;
using System.Buffers.Binary;

namespace Noppes.Fluffle.Search.Business.Similarity;

public static class StreamExtensions
{
    public static async Task WriteInt32LittleEndianAsync(this Stream stream, int value)
    {
        var buffer = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
        await stream.WriteAsync(buffer);
    }

    public static async Task<int> ReadInt32LittleEndianAsync(this Stream stream)
    {
        var buffer = new byte[sizeof(int)];
        await stream.ReadExactlyAsync(buffer);
        var value = BinaryPrimitives.ReadInt32LittleEndian(buffer);

        return value;
    }

    public static async Task ReadInt32LittleEndianAsync(this Stream stream, Memory<int> values, int bufferSize = 4096) =>
        await ReadAsync(stream, buffer => BinaryPrimitives.ReadInt32LittleEndian(buffer.Span), sizeof(int), values, bufferSize);

    public static async Task ReadUInt64LittleEndianAsync(this Stream stream, Memory<ulong> values, int bufferSize = 4096) =>
        await ReadAsync(stream, buffer => BinaryPrimitives.ReadUInt64LittleEndian(buffer.Span), sizeof(ulong), values, bufferSize);

    private static async Task ReadAsync<T>(this Stream stream, Func<Memory<byte>, T> bytesToValue, int size, Memory<T> values, int bufferSize)
    {
        var rentBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            var nValuesThatFitInBuffer = bufferSize / size;
            var nValuesLeftToRead = values.Length;
            while (true)
            {
                var nValuesToReadInBatch = nValuesLeftToRead > nValuesThatFitInBuffer ? nValuesThatFitInBuffer : nValuesLeftToRead;
                var nBytesToReadInBatch = nValuesToReadInBatch * size;

                var buffer = rentBuffer.AsMemory(0, nBytesToReadInBatch);
                await stream.ReadExactlyAsync(buffer);

                var indexOffset = values.Length - nValuesLeftToRead;
                for (var i = 0; i < nValuesToReadInBatch; i++)
                    values.Span[indexOffset + i] = bytesToValue(buffer.Slice(i * size, size));

                nValuesLeftToRead -= nValuesToReadInBatch;

                if (nValuesLeftToRead == 0)
                    break;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentBuffer);
        }
    }

    public static async Task WriteUInt64LittleEndianAsync(this Stream stream, Memory<ulong> values, int bufferSize = 4096) =>
        await WriteAsync(stream, (memory, value) => BinaryPrimitives.WriteUInt64LittleEndian(memory.Span, value), sizeof(ulong), values, bufferSize);

    public static async Task WriteInt32LittleEndianAsync(this Stream stream, Memory<int> values, int bufferSize = 4096) =>
        await WriteAsync(stream, (memory, value) => BinaryPrimitives.WriteInt32LittleEndian(memory.Span, value), sizeof(int), values, bufferSize);

    private static async Task WriteAsync<T>(this Stream stream, Action<Memory<byte>, T> valueToBytes, int size, Memory<T> values, int bufferSize)
    {
        var rentBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            var buffer = rentBuffer.AsMemory(0, bufferSize);
            var offset = 0;
            for (var i = 0; i < values.Length; i++)
            {
                valueToBytes(buffer.Slice(offset, size), values.Span[i]);
                offset += size;

                if (buffer.Length != offset)
                    continue;

                await stream.WriteAsync(buffer);
                offset = 0;
            }

            // Write the remaining bytes
            if (offset != 0)
            {
                await stream.WriteAsync(buffer.Slice(0, offset));
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentBuffer);
        }
    }
}

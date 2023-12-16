using System;

namespace Noppes.Fluffle.Utils;

public class ByteConvert
{
    /// <summary>
    /// Helper method to convert a hash consisting out of bytes into a <see cref="ulong"/>. The
    /// length of the provided array needs to be dividable by 8.
    /// </summary>
    public static ulong[] ToInt64(byte[] hashAsBytes)
    {
        if (hashAsBytes.Length % 8 != 0)
            throw new InvalidOperationException("The provided hash isn't dividable by 8.");

        var hashAsUlongs = new ulong[hashAsBytes.Length / 8];
        for (var i = 0; i < hashAsUlongs.Length; i++)
        {
            var longPart = hashAsBytes.AsSpan(i * 8, 8);
            hashAsUlongs[i] = ToUInt64(longPart);
        }

        return hashAsUlongs;
    }

    /// <summary>
    /// Helper method to convert a hash consisting out of 8 bytes into a <see cref="ulong"/>.
    /// </summary>
    public static ulong ToUInt64(ReadOnlySpan<byte> hashAsBytes)
    {
        if (hashAsBytes.Length != 8)
            throw new ArgumentException($"Array needs to have a length of 8.");

        var hashAsUlong = BitConverter.ToInt64(hashAsBytes);
        unchecked
        {
            return (ulong)hashAsUlong;
        }
    }
}

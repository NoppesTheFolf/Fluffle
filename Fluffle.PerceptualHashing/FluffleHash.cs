using Nitranium.PerceptualHashing;
using Nitranium.PerceptualHashing.Imaging;
using Nitranium.PerceptualHashing.Providers;
using System;
using System.Diagnostics;

namespace Noppes.Fluffle.PerceptualHashing
{
    /// <summary>
    /// Provides perceptual hashing functionality.
    /// </summary>
    public class FluffleHash
    {
        private readonly ImagingProviderFactory _imagingProviderFactory;

        public FluffleHash()
        {
            // Use libvips imaging provider on platforms that are not Linux
            _imagingProviderFactory = new VipsInteropImagingProviderFactory();
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return;

            // Prevent running program on Windows with the system drawing imaging provider
            if (!Debugger.IsAttached)
                throw new InvalidOperationException("This application can't run in production mode on Windows (no libvips).");

            _imagingProviderFactory = new DebugImagingProviderFactory<SystemDrawingImagingProviderFactory>();
        }

        public PHash Create(int size)
        {
            return new PHash
            {
                Size = size,
                Channel = Channel.Average,
                ImagingProviderFactory = _imagingProviderFactory
            };
        }

        /// <summary>
        /// Helper method to convert a hash consisting out of bytes into a <see cref="ulong"/>. The
        /// length of the provided array needs to be dividable by 8 without producing any remainders.
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
}

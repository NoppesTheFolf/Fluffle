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
        /// <summary>
        /// Generates 64-bit hashes using the PHash algorithm.
        /// </summary>
        public PHash Size64 { get; }

        /// <summary>
        /// Generates 256-bit hashes using the PHash algorithm.
        /// </summary>
        public PHash Size256 { get; }

        /// <summary>
        /// Generates 1024-bit hashes using the PHash algorithm.
        /// </summary>
        public PHash Size1024 { get; }

        public FluffleHash()
        {
            ImagingProviderFactory imagingProviderFactory = new VipsInteropImagingProviderFactory();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                if (!Debugger.IsAttached)
                    throw new InvalidOperationException("This application can't run in production mode on Windows (no libvips).");

                imagingProviderFactory = new SystemDrawingImagingProviderFactory();
            }

            Size64 = Create(imagingProviderFactory, 8);
            Size256 = Create(imagingProviderFactory, 32);
            Size1024 = Create(imagingProviderFactory, 128);
        }

        private static PHash Create(ImagingProviderFactory imagingProviderFactory, int size)
        {
            return new PHash
            {
                Size = size,
                Channel = Channel.Average,
                ImagingProviderFactory = imagingProviderFactory
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

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Noppes.Fluffle.Vips
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct InteropVipsThumbnailResult
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string Error;

        public int Width;

        public int Height;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct InteropVipsCenterResult
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string Error;

        public int X;

        public int Y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct InteropVipsImageDimensions
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string Error;

        public int Width;

        public int Height;
    }

    internal class VipsInterop
    {
        private const string LibraryName = "LibFluffleVips.so";

        static VipsInterop()
        {
            if (!File.Exists(LibraryName))
                throw new InvalidOperationException("Library not found in output directory.");

            if (Environment.OSVersion.Platform != PlatformID.Unix)
                throw new InvalidOperationException("Only Unix-based systems are supported.");

            if (!VipsInit())
                throw new InvalidOperationException("Couldn't initialize libvips.");
        }

        [DllImport(LibraryName, CharSet = CharSet.Ansi)]
        private static extern bool VipsInit();

        [DllImport(LibraryName, CharSet = CharSet.Ansi)]
        public static extern InteropVipsThumbnailResult ThumbnailJpeg(string sourceLocation, string destinationLocation, int width, int height, int quality);

        [DllImport(LibraryName, CharSet = CharSet.Ansi)]
        public static extern InteropVipsThumbnailResult ThumbnailWebP(string sourceLocation, string destinationLocation, int width, int height, int quality);

        [DllImport(LibraryName, CharSet = CharSet.Ansi)]
        public static extern InteropVipsThumbnailResult ThumbnailAvif(string sourceLocation, string destinationLocation, int width, int height, int quality);

        [DllImport(LibraryName, CharSet = CharSet.Ansi)]
        public static extern InteropVipsThumbnailResult ThumbnailPpm(string sourceLocation, string destinationLocation, int width, int height);

        [DllImport(LibraryName, CharSet = CharSet.Ansi)]
        public static extern InteropVipsCenterResult Center(string location);

        [DllImport(LibraryName, CharSet = CharSet.Ansi)]
        public static extern InteropVipsImageDimensions GetDimensions(string sourceLocation);
    }
}

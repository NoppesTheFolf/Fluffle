using System;
using System.Collections.Generic;

namespace Noppes.Fluffle.Constants
{
    /// <summary>
    /// File formats recognized by Fluffle.
    /// </summary>
    public enum FileFormatConstant
    {
        Png = 1,
        Jpeg = 2,
        WebM = 3,
        Swf = 4,
        Gif = 5,
        WebP = 6
    }

    public static class FileFormatHelper
    {
        private static readonly HashSet<FileFormatConstant> SupportTransparency = new HashSet<FileFormatConstant>
        {
            FileFormatConstant.Png,
            FileFormatConstant.WebM,
            FileFormatConstant.WebP
        };

        /// <summary>
        /// Whether the file format supports transparency (an alpha channel) or not.
        /// </summary>
        public static bool SupportsTransparency(this FileFormatConstant fileFormat) => SupportTransparency.Contains(fileFormat);

        /// <summary>
        /// Maps the provided extension to a <see cref="FileFormatConstant"/>.
        /// </summary>
        public static FileFormatConstant GetFileFormatFromExtension(string extension)
        {
            extension = extension.Trim();

            // Remove the '.' before the file extension
            if (extension.StartsWith("."))
                extension = extension[1..];

            // Change jpeg to jpg
            if (extension == "jpeg")
                extension = "jpg";

            // Make the matching case insensitive
            extension = extension.ToLowerInvariant();

            return extension switch
            {
                "png" => FileFormatConstant.Png,
                "jpg" => FileFormatConstant.Jpeg,
                "webm" => FileFormatConstant.WebM,
                "swf" => FileFormatConstant.Swf,
                "gif" => FileFormatConstant.Gif,
                "webp" => FileFormatConstant.WebP,
                _ => throw new InvalidOperationException($"Extension `{extension}` could not be found")
            };
        }

        public static FileFormatConstant GetFileFormatFromMimeType(string mimeType)
        {
            mimeType = mimeType.ToLowerInvariant().Trim();

            return mimeType switch
            {
                "image/jpeg" => FileFormatConstant.Jpeg,
                "image/png" => FileFormatConstant.Png,
                "image/webp" => FileFormatConstant.WebP,
                "image/gif" => FileFormatConstant.Gif,
                "video/webm" => FileFormatConstant.WebM,
                "application/x-shockwave-flash" => FileFormatConstant.Swf,
                _ => throw new InvalidOperationException($"MIME type `{mimeType}` could not be found")
            };
        }
    }
}

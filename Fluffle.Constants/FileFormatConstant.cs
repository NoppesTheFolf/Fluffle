using System;

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
        /// <summary>
        /// Maps the provided extension to a <see cref="FileFormatConstant"/>.
        /// </summary>
        public static FileFormatConstant GetFileFormat(string extension)
        {
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
    }
}

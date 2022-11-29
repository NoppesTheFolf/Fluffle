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
        WebP = 6,
        Html = 7,
        Pdf = 8,
        Rtf = 9,
        Txt = 10,
        Doc = 11,
        Docx = 12,
        Odt = 13,
        Mp3 = 14,
        Wav = 15,
        Mid = 16,
        Binary = 17,
        Mp4 = 18
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

        private static readonly HashSet<FileFormatConstant> SupportAnimation = new HashSet<FileFormatConstant>
        {
            FileFormatConstant.Gif,
            FileFormatConstant.WebP
            // Technically PNGs should be part of this list too... but we don't really want to download all full sized PNGs
        };

        /// <summary>
        /// Whether the file format supports animations (sequence of images) or not.
        /// </summary>
        public static bool SupportsAnimation(this FileFormatConstant fileFormat) => SupportAnimation.Contains(fileFormat);

        /// <summary>
        /// Maps the provided extension to a <see cref="FileFormatConstant"/>.
        /// </summary>
        public static FileFormatConstant GetFileFormatFromExtension(string extension, FileFormatConstant? fallback = null)
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
                "html" => FileFormatConstant.Html,
                "htm" => FileFormatConstant.Html,
                "pdf" => FileFormatConstant.Pdf,
                "rtf" => FileFormatConstant.Rtf,
                "txt" => FileFormatConstant.Txt,
                "doc" => FileFormatConstant.Doc,
                "docx" => FileFormatConstant.Docx,
                "odt" => FileFormatConstant.Odt,
                "mp3" => FileFormatConstant.Mp3,
                "wav" => FileFormatConstant.Wav,
                "mid" => FileFormatConstant.Mid,
                "bin" => FileFormatConstant.Binary,
                "" => FileFormatConstant.Binary,
                "mp4" => FileFormatConstant.Mp4,
                _ => fallback ?? throw new InvalidOperationException($"Extension `{extension}` could not be found")
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
                "text/html" => FileFormatConstant.Html,
                _ => throw new InvalidOperationException($"MIME type `{mimeType}` could not be found")
            };
        }
    }
}

using System;

namespace Noppes.Fluffle.Constants
{
    /// <summary>
    /// Image formats considered by Fluffle.
    /// </summary>
    public enum ImageFormatConstant
    {
        Jpeg = 1,
        WebP = 2
    }

    /// <summary>
    /// Some helper methods to deal with image formats.
    /// </summary>
    public static class ImageFormatConstantExtensions
    {
        /// <summary>
        /// Gets the file extension commonly associated with the given image format.
        /// </summary>
        public static string GetFileExtension(this ImageFormatConstant imageFormatConstant)
        {
            return imageFormatConstant switch
            {
                ImageFormatConstant.WebP => "webp",
                ImageFormatConstant.Jpeg => "jpg",
                _ => throw new ArgumentOutOfRangeException(nameof(imageFormatConstant))
            };
        }

        /// <summary>
        /// Gets the mime type of the the given image format.
        /// </summary>
        public static string GetMimeType(this ImageFormatConstant imageFormatConstant)
        {
            return imageFormatConstant switch
            {
                ImageFormatConstant.WebP => "image/webp",
                ImageFormatConstant.Jpeg => "image/jpeg",
                _ => throw new ArgumentOutOfRangeException(nameof(imageFormatConstant))
            };
        }
    }
}

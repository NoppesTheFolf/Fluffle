using Noppes.Fluffle.Constants;
using System;

namespace Noppes.Fluffle.Thumbnail;

/// <summary>
/// Base class for classes which are able to generate thumbnails.
/// </summary>
public abstract class FluffleThumbnail
{
    /// <summary>
    /// Generates a thumbnail of the specified format, size and quality. The width or height are
    /// automatically calculated so that the smallest dimension is always equal to at least the
    /// given target size. So a source image of 200x400, with a target size of 100 pixels, will
    /// create a thumbnail of 100x200.
    /// </summary>
    public FluffleThumbnailResult Generate(string sourceLocation, string destinationLocation, int targetSize, ImageFormatConstant imageFormatConstant, int quality)
    {
        if (targetSize < 1)
            throw new InvalidOperationException();

        var imageDimensions = GetDimensions(sourceLocation);
        var (width, height) = CalculateThumbnailSize(imageDimensions, targetSize);

        Func<FluffleThumbnailResult> thumbnailFunction = imageFormatConstant switch
        {
            ImageFormatConstant.Jpeg => () => ThumbnailJpeg(sourceLocation, destinationLocation, width, height, quality),
            ImageFormatConstant.WebP => () => ThumbnailWebP(sourceLocation, destinationLocation, width, height, quality),
            _ => throw new ArgumentOutOfRangeException($"Thumbnailing an image with format `{imageFormatConstant}` is not supported.")
        };
        var thumbnailResult = thumbnailFunction();

        return thumbnailResult;
    }

    /// <summary>
    /// Creates a thumbnail encoded using JPEG.
    /// </summary>
    public abstract FluffleThumbnailResult ThumbnailJpeg(string sourceLocation, string destinationLocation, int width, int height, int quality);

    /// <summary>
    /// Creates a thumbnail encoded using WebP.
    /// </summary>
    public abstract FluffleThumbnailResult ThumbnailWebP(string sourceLocation, string destinationLocation, int width, int height, int quality);

    /// <summary>
    /// Gets the image dimensions of the image located at the given location.
    /// </summary>
    public abstract ImageDimensions GetDimensions(string sourceLocation);

    private static (int width, int height) CalculateThumbnailSize(ImageDimensions dimensions, int targetSize)
    {
        static int DetermineSize(int sizeOne, int sizeTwo, int sizeOneTarget)
        {
            var aspectRatio = (double)sizeOneTarget / sizeOne;

            return (int)Math.Round(aspectRatio * sizeTwo);
        }

        if (dimensions.Width == dimensions.Height)
            return (targetSize, targetSize);

        return dimensions.Width > dimensions.Height
            ? (DetermineSize(dimensions.Height, dimensions.Width, targetSize), targetSize)
            : (targetSize, DetermineSize(dimensions.Width, dimensions.Height, targetSize));
    }
}

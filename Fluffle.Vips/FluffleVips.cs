using System;

namespace Noppes.Fluffle.Vips;

public struct VipsThumbnailResult
{
    /// <summary>
    /// Width of the thumbnail in pixels.
    /// </summary>
    public int Width;

    /// <summary>
    /// Height of the thumbnail in pixels.
    /// </summary>
    public int Height;
}

public struct VipsCenterResult
{
    /// <summary>
    /// Percentage of horizontal offset which could be used when cropping the image to a square
    /// to get the content centered. Note that this is relative to the image its full width. So
    /// a 100% offset means 100% of the remaining (non-visible) space. Take a 400x300 image, a
    /// 100% offset would mean 100px as that would make a 300x300 square.
    /// </summary>
    public int X;

    /// <summary>
    /// Percentage of vertical offset which could be used when cropping the image to a square
    /// to get the content centered. Note that this is relative to the image its full height. So
    /// a 100% offset means 100% of the remaining (non-visible) space. Take a 300x500 image, a
    /// 100% offset would mean 200px as that would make a 300x300 square.
    /// </summary>
    public int Y;
}

public struct VipsImageDimensions
{
    /// <summary>
    /// Width of the image in pixels.
    /// </summary>
    public int Width;

    /// <summary>
    /// Height of the image in pixels.
    /// </summary>
    public int Height;
}

/// <summary>
/// Basically a mirror of the <see cref="VipsInterop"/> class, but hides the way errors are handled.
/// </summary>
public static class FluffleVips
{
    /// <summary>
    /// Creates a JPEG thumbnail with the given parameters.
    /// </summary>
    public static VipsThumbnailResult ThumbnailJpeg(string sourceLocation, string destinationLocation, int width, int height, int quality) =>
        Thumbnail(() => VipsInterop.ThumbnailJpeg(sourceLocation, destinationLocation, width, height, quality));

    /// <summary>
    /// Creates a WebP thumbnail with the given parameters.
    /// </summary>
    public static VipsThumbnailResult ThumbnailWebP(string sourceLocation, string destinationLocation, int width, int height, int quality) =>
        Thumbnail(() => VipsInterop.ThumbnailWebP(sourceLocation, destinationLocation, width, height, quality));

    /// <summary>
    /// Creates a AVIF thumbnail with the given parameters.
    /// </summary>
    public static VipsThumbnailResult ThumbnailAvif(string sourceLocation, string destinationLocation, int width, int height, int quality) =>
        Thumbnail(() => VipsInterop.ThumbnailAvif(sourceLocation, destinationLocation, width, height, quality));

    /// <summary>
    /// Creates a PPM thumbnail with the given parameters.
    /// </summary>
    public static VipsThumbnailResult ThumbnailPpm(string sourceLocation, string destinationLocation, int width, int height) =>
        Thumbnail(() => VipsInterop.ThumbnailPpm(sourceLocation, destinationLocation, width, height));

    private static VipsThumbnailResult Thumbnail(Func<InteropVipsThumbnailResult> thumbnailFunc)
    {
        var result = thumbnailFunc();

        HandleError(result.Error);

        return new VipsThumbnailResult
        {
            Width = result.Width,
            Height = result.Height
        };
    }

    /// <summary>
    /// Calculates the center of the given image intelligently.
    /// </summary>
    public static VipsCenterResult Center(string location)
    {
        var result = VipsInterop.Center(location);

        HandleError(result.Error);

        return new VipsCenterResult
        {
            X = result.X,
            Y = result.Y
        };
    }

    /// <summary>
    /// Gets the dimensions (width and height) of the given image.
    /// </summary>
    public static VipsImageDimensions GetDimensions(string sourceLocation)
    {
        var result = VipsInterop.GetDimensions(sourceLocation);

        HandleError(result.Error);

        return new VipsImageDimensions
        {
            Width = result.Width,
            Height = result.Height
        };
    }

    private static void HandleError(string error)
    {
        if (!string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException(error);
    }
}

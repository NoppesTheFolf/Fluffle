using Noppes.Fluffle.Vips;
using System;

namespace Noppes.Fluffle.Thumbnail;

/// <summary>
/// A <see cref="FluffleThumbnail"/> implementation which uses libvips with interop as imaging backend.
/// </summary>
public class VipsFluffleThumbnail : FluffleThumbnail
{
    /// <inheritdoc/>
    public override FluffleThumbnailResult ThumbnailJpeg(string sourceLocation, string destinationLocation, int width, int height, int quality) =>
        Thumbnail(sourceLocation, destinationLocation, width, height, quality, FluffleVips.ThumbnailJpeg);

    /// <inheritdoc/>
    public override FluffleThumbnailResult ThumbnailWebP(string sourceLocation, string destinationLocation, int width, int height, int quality) =>
        Thumbnail(sourceLocation, destinationLocation, width, height, quality, FluffleVips.ThumbnailWebP);

    private static FluffleThumbnailResult Thumbnail(string sourceLocation, string destinationLocation, int width, int height,
        int quality, Func<string, string, int, int, int, VipsThumbnailResult> thumbnailFunc)
    {
        var thumbnailResult = thumbnailFunc(sourceLocation, destinationLocation, width, height, quality);
        var centerResult = FluffleVips.Center(sourceLocation);

        return new FluffleThumbnailResult
        {
            Width = thumbnailResult.Width,
            Height = thumbnailResult.Height,
            CenterX = centerResult.X,
            CenterY = centerResult.Y
        };
    }

    /// <inheritdoc/>
    public override ImageDimensions GetDimensions(string sourceLocation)
    {
        var dimensions = FluffleVips.GetDimensions(sourceLocation);

        return new ImageDimensions(dimensions.Width, dimensions.Height);
    }
}

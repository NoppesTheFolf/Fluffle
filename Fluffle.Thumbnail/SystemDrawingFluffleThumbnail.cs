﻿using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Noppes.Fluffle.Thumbnail
{
    /// <summary>
    /// A <see cref="FluffleThumbnail"/> implementation which uses System.Drawing as imaging backend.
    /// </summary>
    public class SystemDrawingFluffleThumbnail : FluffleThumbnail
    {
        /// <inheritdoc/>
        public override FluffleThumbnailResult ThumbnailJpeg(string sourceLocation, string destinationLocation, int width, int height, int quality)
        {
            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(width), "A thumbnail its width cannot be less than 1 pixel.");

            if (height < 1)
                throw new ArgumentOutOfRangeException(nameof(height), "A thumbnail its height cannot be less than 1 pixel.");

            using var image = Image.FromFile(sourceLocation);

            using var thumbnailImage = image.GetThumbnailImage(width, height, null, IntPtr.Zero);
            thumbnailImage.Save(destinationLocation, ImageFormat.Jpeg);

            var result = new FluffleThumbnailResult
            {
                Width = thumbnailImage.Width,
                Height = thumbnailImage.Height
            };

            if (thumbnailImage.Width == thumbnailImage.Height)
                return result;

            // Center either the X or Y axis unintelligently. Whatever. This implementation is not
            // used in production anyway
            result.CenterX = thumbnailImage.Width > thumbnailImage.Height ? 50 : 0;
            result.CenterY = thumbnailImage.Height > thumbnailImage.Width ? 50 : 0;

            return result;
        }

        /// <inheritdoc/>
        public override FluffleThumbnailResult ThumbnailWebP(string sourceLocation, string destinationLocation, int width, int height, int quality)
        {
            throw new NotImplementedException("System.Drawing doesn't have support for WebP images.");
        }

        /// <inheritdoc/>
        public override ImageDimensions GetDimensions(string sourceLocation)
        {
            using var image = Image.FromFile(sourceLocation);

            return new ImageDimensions(image.Width, image.Height);
        }
    }
}
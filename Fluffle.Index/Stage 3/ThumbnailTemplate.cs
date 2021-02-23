using Noppes.Fluffle.Constants;
using System;

namespace Noppes.Fluffle.Index
{
    public class ThumbnailTemplate
    {
        public int Size { get; }

        public int Quality { get; }

        public ImageFormatConstant Format { get; }

        public string Discriminator { get; }

        public ThumbnailTemplate(ImageFormatConstant format, int size, int quality, string discriminator)
        {
            if (string.IsNullOrWhiteSpace(discriminator))
                throw new ArgumentException(null, nameof(discriminator));

            if (quality < 1)
                throw new ArgumentException(null, nameof(quality));

            if (size < 1)
                throw new ArgumentException(null, nameof(size));

            Size = size;
            Quality = quality;
            Format = format;
            Discriminator = discriminator;
        }
    }
}

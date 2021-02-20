using Nitranium.PerceptualHashing;
using Nitranium.PerceptualHashing.Imaging;
using Nitranium.PerceptualHashing.Imaging.Ppm;
using Nitranium.PerceptualHashing.Utils;
using Noppes.Fluffle.Vips;

namespace Noppes.Fluffle.PerceptualHashing
{
    /// <summary>
    /// Provides an imaging provider implementation using libvips interop.
    /// </summary>
    public class VipsImagingProvider : FileImagingProvider
    {
        public override Image Prepare(string inputPath, in Resolution resolution)
        {
            using var temporaryFile = new TemporaryFile();

            FluffleVips.ThumbnailPpm(inputPath, temporaryFile.Location, resolution.Width, resolution.Height);

            return new PpmReader(temporaryFile.Location);
        }
    }

    /// <summary>
    /// Creates instances of <see cref="VipsImagingProvider"/>.
    /// </summary>
    public class VipsInteropImagingProviderFactory : ImagingProviderFactory
    {
        public override ImagingProvider CreateImagingProvider()
        {
            return new VipsImagingProvider();
        }
    }
}

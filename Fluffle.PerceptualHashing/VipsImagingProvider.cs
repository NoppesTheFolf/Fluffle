using Nitranium.PerceptualHashing;
using Nitranium.PerceptualHashing.Imaging;
using Nitranium.PerceptualHashing.Imaging.Ppm;
using Nitranium.PerceptualHashing.Utils;
using Noppes.Fluffle.Vips;
using System.Collections.Generic;
using System.Threading;

namespace Noppes.Fluffle.PerceptualHashing
{
    /// <summary>
    /// Provides an imaging provider implementation using libvips interop.
    /// </summary>
    public class VipsImagingProvider : FileImagingProvider
    {
        private readonly SemaphoreSlim _mutex;
        private readonly IDictionary<Resolution, TemporaryFile> _cache;

        public VipsImagingProvider()
        {
            _mutex = new SemaphoreSlim(1);
            _cache = new Dictionary<Resolution, TemporaryFile>();
        }

        public override Image Prepare(string inputPath, in Resolution resolution)
        {
            _mutex.Wait();
            try
            {
                if (!_cache.TryGetValue(resolution, out var temporaryFile))
                {
                    temporaryFile = new TemporaryFile();
                    FluffleVips.ThumbnailPpm(inputPath, temporaryFile.Location, resolution.Width, resolution.Height);

                    _cache[resolution] = temporaryFile;
                }

                return new PpmReader(temporaryFile.Location);
            }
            finally
            {
                _mutex.Release();
            }
        }

        public override void Dispose()
        {
            foreach (var temporaryFile in _cache.Values)
                temporaryFile.Dispose();
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

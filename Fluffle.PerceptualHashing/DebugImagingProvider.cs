using Nitranium.PerceptualHashing;
using Nitranium.PerceptualHashing.Imaging;
using System.IO;

namespace Noppes.Fluffle.PerceptualHashing
{
    public class DebugImagingProvider : ImagingProvider
    {
        private readonly ImagingProvider _imagingProvider;

        public DebugImagingProvider(ImagingProvider imagingProvider)
        {
            _imagingProvider = imagingProvider;
        }

        public override Image Prepare(Stream inputStream, in Resolution resolution)
        {
            return _imagingProvider.Prepare(inputStream, in resolution);
        }

        public override Image Prepare(string inputPath, in Resolution resolution)
        {
            return _imagingProvider.Prepare(inputPath, in resolution);
        }
    }

    public class DebugImagingProviderFactory<T> : ImagingProviderFactory where T : ImagingProviderFactory, new()
    {
        private readonly ImagingProviderFactory _factory;

        public DebugImagingProviderFactory()
        {
            _factory = new T();
        }

        public override ImagingProvider CreateImagingProvider()
        {
            return new DebugImagingProvider(_factory.CreateImagingProvider());
        }
    }
}

using Nitranium.PerceptualHashing;
using Nitranium.PerceptualHashing.Exceptions;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.PerceptualHashing;
using Noppes.Fluffle.Thumbnail;
using SerilogTimings;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index
{
    public class ImageHasher : ImageConsumer
    {
        private const int DifferenceThreshold = 12;

        private readonly FluffleHash _fluffleHash;
        private readonly FluffleThumbnail _thumbnailer;

        public ImageHasher(FluffleClient fluffleClient, FluffleHash fluffleHash,
            FluffleThumbnail thumbnailer) : base(fluffleClient)
        {
            _fluffleHash = fluffleHash;
            _thumbnailer = thumbnailer;
        }

        public override Task<ChannelImage> ConsumeAsync(ChannelImage image)
        {
            var dimensions = _thumbnailer.GetDimensions(image.File.Location);
            var max = Math.Max(dimensions.Width, dimensions.Height);
            var min = Math.Min(dimensions.Width, dimensions.Height);
            var difference = max / (double)min;

            // TODO: This should not be considered an error, instead it should simply not be considered for indexation next time
            if (difference > DifferenceThreshold)
            {
                image.Error = "The image its aspect ratio is too extreme to process.";
                return Task.FromResult(image);
            }

            try
            {
                using (Operation.Time("[{platformName}, {idOnPlatform}, 2/5] Computed perceptual hashes", image.Content.PlatformName, image.Content.IdOnPlatform))
                {
                    // Unlike in the search API, here we circumvent the optimizations used in the libvips
                    // imaging provider by creating multiple separate hashers.
                    using var hasher64 = _fluffleHash.Create(8).For(image.File.Location);
                    using var hasher256 = _fluffleHash.Create(32).For(image.File.Location);
                    using var hasher1024 = _fluffleHash.Create(128).For(image.File.Location);

                    image.Hashes = new PutImageIndexModel.ImageHashesModel
                    {
                        PhashRed64 = hasher64.ComputeHash(Channel.Red),
                        PhashGreen64 = hasher64.ComputeHash(Channel.Green),
                        PhashBlue64 = hasher64.ComputeHash(Channel.Blue),
                        PhashAverage64 = hasher64.ComputeHash(Channel.Average),
                        PhashRed256 = hasher256.ComputeHash(Channel.Red),
                        PhashGreen256 = hasher256.ComputeHash(Channel.Green),
                        PhashBlue256 = hasher256.ComputeHash(Channel.Blue),
                        PhashAverage256 = hasher256.ComputeHash(Channel.Average),
                        PhashRed1024 = hasher1024.ComputeHash(Channel.Red),
                        PhashGreen1024 = hasher1024.ComputeHash(Channel.Green),
                        PhashBlue1024 = hasher1024.ComputeHash(Channel.Blue),
                        PhashAverage1024 = hasher1024.ComputeHash(Channel.Average)
                    };
                }
            }
            catch (ConvertException e)
            {
                image.Error = e.ToString();
            }

            return Task.FromResult(image);
        }
    }
}

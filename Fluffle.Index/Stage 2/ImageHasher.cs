using Nitranium.PerceptualHashing;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.PerceptualHashing;
using SerilogTimings;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index
{
    public class ImageHasher : ImageConsumer
    {
        private readonly FluffleHash _fluffleHash;

        public ImageHasher(FluffleClient fluffleClient, FluffleHash fluffleHash) : base(fluffleClient)
        {
            _fluffleHash = fluffleHash;
        }

        public override Task<ChannelImage> ConsumeAsync(ChannelImage image)
        {
            using (Operation.Time("[{platformName}, {idOnPlatform}, 2/5] Computed perceptual hashes", image.Content.PlatformName, image.Content.IdOnPlatform))
            {
                using var hasher64 = _fluffleHash.Size64.For(image.File.Location);
                using var hasher256 = _fluffleHash.Size256.For(image.File.Location);
                using var hasher1024 = _fluffleHash.Size1024.For(image.File.Location);

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

            return Task.FromResult(image);
        }
    }
}

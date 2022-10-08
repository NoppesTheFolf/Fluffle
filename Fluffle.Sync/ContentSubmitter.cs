using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using SerilogTimings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Sync
{
    public class ContentSubmitter : SyncConsumer
    {
        private readonly PlatformModel _platform;
        private readonly FluffleClient _client;

        public ContentSubmitter(PlatformModel platform, FluffleClient client)
        {
            _platform = platform;
            _client = client;
        }

        public override async Task<ICollection<PutContentModel>> ConsumeAsync(ICollection<PutContentModel> models)
        {
            if (!models.Any())
                return models;

            using var _ = Operation.Time($"Submitting {{contentCount}} content {(models.Count == 1 ? "piece" : "pieces")}", models.Count);

            await HttpResiliency.RunAsync(async () =>
            {
                await _client.PutContentAsync(_platform.NormalizedName, models);
            });

            return models;
        }
    }
}

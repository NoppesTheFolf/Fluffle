using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.Sync;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurAffinitySync
{
    internal class SyncClient : SyncClient<SyncClient, FurAffinityContentProducer, FaSubmission>
    {
        private static async Task Main() => await RunAsync("Fur Affinity", async (configuration, services) =>
        {
            var client = await new FurAffinityClientFactory(configuration).CreateAsync("fluffle-fur-affinity-sync");

            services.AddSingleton(client);
        });
    }
}

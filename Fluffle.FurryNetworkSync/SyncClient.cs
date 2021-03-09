using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Sync;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurryNetworkSync
{
    internal class SyncClient : SyncClient<SyncClient, FurryNetworkContentProducer, FnSubmission>
    {
        private static async Task Main() => await RunAsync("Furry Network", async (configuration, services) =>
        {
            var client = await new FurryNetworkClientFactory(configuration).CreateAsync("fluffle-furry-network-sync");

            services.AddSingleton(client);
        });
    }
}

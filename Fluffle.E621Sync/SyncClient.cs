using Microsoft.Extensions.DependencyInjection;
using Noppes.E621;
using Noppes.Fluffle.Sync;
using System.Threading.Tasks;

namespace Noppes.Fluffle.E621Sync
{
    internal class SyncClient : SyncClient<SyncClient, E621ContentProducer, Post>
    {
        private static async Task Main() => await RunAsync("e621", async (configuration, services) =>
        {
            var e621Client = await new E621ClientFactory(configuration)
                .CreateAsync("fluffle-e621sync");

            services.AddSingleton(e621Client);
        });
    }
}

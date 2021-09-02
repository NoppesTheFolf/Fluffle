using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Sync;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurryNetworkSync
{
    internal class SyncClient : SyncClient<SyncClient, FurryNetworkContentProducer, FnSubmission>
    {
        public SyncClient(IServiceProvider services) : base(services)
        {
        }

        private static async Task Main(string[] args) => await RunAsync(args, "Furry Network", (configuration, services) =>
        {
            var client = new FurryNetworkClientFactory(configuration)
                .CreateAsync(2000).Result;

            services.AddSingleton(client);
        });
    }
}

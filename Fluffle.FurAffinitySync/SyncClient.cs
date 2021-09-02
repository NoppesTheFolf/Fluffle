using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.Sync;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurAffinitySync
{
    internal class SyncClient : SyncClient<SyncClient, FurAffinityContentProducer, FaSubmission>
    {
        public SyncClient(IServiceProvider services) : base(services)
        {
        }

        private static async Task Main(string[] args) => await RunAsync(args, "Fur Affinity", (configuration, services) =>
        {
            var client = new FurAffinityClientFactory(configuration)
                .CreateAsync(500).Result;

            services.AddSingleton(client);
        });
    }
}

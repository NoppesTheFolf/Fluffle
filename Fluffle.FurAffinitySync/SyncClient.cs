using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.Sync;
using System;

namespace Noppes.Fluffle.FurAffinitySync
{
    internal class SyncClient : SyncClient<SyncClient, FurAffinityContentProducer, FaSubmission>
    {
        public SyncClient(IServiceProvider services) : base(services)
        {
        }

        private static void Main(string[] args) => Run(args, "Fur Affinity", (configuration, services) =>
        {
            var client = new FurAffinityClientFactory(configuration)
                .CreateAsync(500).Result;

            services.AddSingleton(client);
        });
    }
}

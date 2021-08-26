using Microsoft.Extensions.DependencyInjection;
using Noppes.E621;
using Noppes.Fluffle.Sync;
using System;

namespace Noppes.Fluffle.E621Sync
{
    internal class SyncClient : SyncClient<SyncClient, E621ContentProducer, Post>
    {
        public SyncClient(IServiceProvider services) : base(services)
        {
        }

        private static void Main(string[] args) => Run(args, "e621", (configuration, services) =>
        {
            var e621Client = new E621ClientFactory(configuration)
                .CreateAsync("fluffle-e621sync", 1000).Result;

            services.AddSingleton(e621Client);
        });
    }
}

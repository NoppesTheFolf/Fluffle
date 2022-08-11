using Microsoft.Extensions.DependencyInjection;
using Noppes.E621;
using Noppes.Fluffle.Sync;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.E621Sync
{
    internal class SyncClient : SyncClient<SyncClient, E621ContentProducer, Post>
    {
        private const string ApplicationName = "e621-sync";

        public SyncClient(IServiceProvider services) : base(services)
        {
        }

        private static async Task Main(string[] args) => await RunAsync(args, "e621", (configuration, services) =>
        {
            var e621Client = new E621ClientFactory(configuration).CreateAsync(1000, ApplicationName).Result;

            services.AddSingleton(e621Client);
        });
    }
}

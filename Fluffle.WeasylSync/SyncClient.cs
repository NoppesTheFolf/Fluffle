using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Sync;
using Noppes.Fluffle.Weasyl.Models;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.WeasylSync
{
    internal class SyncClient : SyncClient<SyncClient, WeasylContentProducer, Submission>
    {
        public SyncClient(IServiceProvider services) : base(services)
        {
        }

        private static async Task Main(string[] args) => await RunAsync(args, "Weasyl", (configuration, services) =>
        {
            var weasylClient = new WeasylClientFactory(configuration).CreateAsync(1000).Result;

            services.AddSingleton(weasylClient);
        });
    }
}

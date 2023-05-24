using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Sync;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurryNetworkSync;

internal class SyncClient : SyncClient<SyncClient, FurryNetworkContentProducer, FnSubmission>
{
    private const string ApplicationName = "furry-network-sync";

    public SyncClient(IServiceProvider services) : base(services)
    {
    }

    private static async Task Main(string[] args) => await RunAsync(args, ApplicationName.Replace("-", "_").Pascalize(), "Furry Network", (configuration, services) =>
    {
        var client = new FurryNetworkClientFactory(configuration).CreateAsync(2000, ApplicationName).Result;

        services.AddSingleton(client);
    });
}

using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Service;
using Noppes.Fluffle.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Sync
{
    public class SyncClient<TService, TSyncService, TContent> : Service<TService>
        where TService : SyncClient<TService, TSyncService, TContent>
        where TSyncService : ContentProducer<TContent>
    {
        private PlatformModel Platform { get; set; }

        public static async Task RunAsync(string platformName, Func<FluffleConfiguration, IServiceCollection, Task> configureAsync = null)
        {
            await RunAsync(async (client, configuration, services) =>
            {
                var mainConfiguration = configuration.Get<MainConfiguration>();
                var fluffleClient = new FluffleClient(mainConfiguration.Url, mainConfiguration.ApiKey);
                services.AddSingleton(fluffleClient);

                client.Platform = await HttpResiliency.RunAsync(async () => await fluffleClient.GetPlatformAsync(platformName));
                services.AddSingleton(client.Platform);

                services.AddTransient<ContentSubmitter>();
                services.AddTransient<TSyncService>();

                if (configureAsync != null)
                    await configureAsync(configuration, services);
            });
        }

        protected override async Task RunAsync()
        {
            Log.Information($"Starting {Platform.Name} syncing client...");

            var manager = new ProducerConsumerManager<ICollection<PutContentModel>>(Services, 20);
            manager.AddProducer<TSyncService>(1);
            manager.AddFinalConsumer<ContentSubmitter>(1);

            await manager.RunAsync();
        }
    }
}

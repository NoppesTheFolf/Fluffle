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
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Sync
{
    public class SyncClient<TService, TContentProducer, TContent> : Service<TService>
        where TService : SyncClient<TService, TContentProducer, TContent>
        where TContentProducer : ContentProducer<TContent>
    {
        protected PlatformModel Platform;
        protected FluffleClient FluffleClient;

        public SyncClient(IServiceProvider services) : base(services)
        {
        }

        public static void Run(string[] args, string platformName, Action<FluffleConfiguration, IServiceCollection> configure = null)
        {
            Run(args, (configuration, services) =>
            {
                var mainConfiguration = configuration.Get<MainConfiguration>();
                var fluffleClient = new FluffleClient(mainConfiguration.Url, mainConfiguration.ApiKey);
                services.AddSingleton(fluffleClient);

                var platformModel = HttpResiliency.RunAsync(
                    async () => await fluffleClient.GetPlatformAsync(platformName)).Result;
                services.AddSingleton(platformModel);

                services.AddTransient(typeof(SyncStateService<>));
                services.AddTransient<ContentSubmitter>();
                services.AddTransient<TContentProducer>();

                configure?.Invoke(configuration, services);
            });
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Platform = Services.GetRequiredService<PlatformModel>();
            FluffleClient = Services.GetRequiredService<FluffleClient>();

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information($"Starting {Platform.Name} syncing client...");

            var manager = new ProducerConsumerManager<ICollection<PutContentModel>>(Services, 20);
            manager.AddProducer<TContentProducer>(1);
            manager.AddFinalConsumer<ContentSubmitter>(1);

            await manager.RunAsync();
        }
    }
}

using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.B2;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.E621Sync;
using Noppes.Fluffle.FurryNetworkSync;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.PerceptualHashing;
using Noppes.Fluffle.Service;
using Noppes.Fluffle.Thumbnail;
using Noppes.Fluffle.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index
{
    public class IndexService : Service.Service
    {
        private const string UserAgent = "fluffle-index";

        private Dictionary<PlatformConstant, DownloadClient> DownloadClients { get; set; }

        private static async Task Main() => await Service<IndexService>.RunAsync(async (client, configuration, services) =>
        {
            var mainConf = configuration.Get<MainConfiguration>();
            var fluffleClient = new FluffleClient(mainConf.Url, mainConf.ApiKey);
            services.AddSingleton(fluffleClient);

            var fluffleHash = new FluffleHash();
            services.AddSingleton(fluffleHash);
            services.AddSingleton(_ => new FluffleHashSelfTestRunner(fluffleHash)
            {
                Log = Log.Information
            });

            var thumbConf = configuration.Get<ThumbnailConfiguration>();
            var b2Conf = configuration.Get<BackblazeB2Configuration>();
            var b2Client = new B2Client(b2Conf.ApplicationKeyId, b2Conf.ApplicationKey, thumbConf.BaseUrl);
            services.AddSingleton(b2Client);
            services.AddSingleton(new B2ThumbnailStorage(b2Client, thumbConf.Salt));

            services.AddSingleton<FluffleThumbnail>(_ =>
            {
                if (Debugger.IsAttached && OperatingSystem.IsWindows())
                    return new SystemDrawingFluffleThumbnail();

                if (!OperatingSystem.IsLinux())
                    throw new InvalidOperationException("Can't run in a non-Linux environment in production.");

                return new VipsFluffleThumbnail();
            });

            var e621Client = await new E621ClientFactory(configuration).CreateAsync(UserAgent);
            var furryNetworkClient = await new FurryNetworkClientFactory(configuration).CreateAsync(UserAgent);
            client.DownloadClients = new Dictionary<PlatformConstant, DownloadClient>
            {
                { PlatformConstant.E621, new DownloadClient(null, (url, _) => e621Client.GetStreamAsync(url)) },
                { PlatformConstant.FurryNetwork, new DownloadClient(3.Seconds(), (url, _) => furryNetworkClient.GetStreamAsync(url)) },
            };

            services.AddTransient<ImageHasher>();
            services.AddTransient<Thumbnailer>();
            services.AddTransient<ThumbnailPublisher>();
            services.AddTransient<IndexPublisher>();
        });

        protected override async Task RunAsync()
        {
            var testRunner = Services.GetRequiredService<FluffleHashSelfTestRunner>();
            testRunner.Run();

            var fluffleClient = Services.GetRequiredService<FluffleClient>();

            var manager = new ProducerConsumerManager<ChannelImage>(Services, 20);

            var platforms = await HttpResiliency.RunAsync(() => fluffleClient.GetPlatformsAsync());
            foreach (var platform in platforms)
            {
                var sourceConstant = (PlatformConstant)platform.Id;
                if (!DownloadClients.TryGetValue(sourceConstant, out var downloadClient))
                {
                    Log.Warning("There exists no download client for {platformName}.", platform.Name);
                    continue;
                }

                Log.Information("[{platformName}] Starting indexing...", platform.Name);
                manager.AddProducer(1, () => new ImageDownloader(fluffleClient, platform, downloadClient));
            }

            manager.AddConsumer<ImageHasher>(2, 20);
            manager.AddConsumer<Thumbnailer>(2, 20);
            manager.AddConsumer<ThumbnailPublisher>(8, 20);
            manager.AddFinalConsumer<IndexPublisher>(8);

            await manager.RunAsync();
        }
    }
}

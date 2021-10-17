using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Noppes.Fluffle.B2;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.E621Sync;
using Noppes.Fluffle.FurAffinitySync;
using Noppes.Fluffle.FurryNetworkSync;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.PerceptualHashing;
using Noppes.Fluffle.Service;
using Noppes.Fluffle.Thumbnail;
using Noppes.Fluffle.TwitterSync;
using Noppes.Fluffle.Utils;
using Noppes.Fluffle.WeasylSync;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index
{
    public class IndexService : Service.Service
    {
        private IndexConfiguration Configuration { get; set; }
        private Dictionary<PlatformConstant, (DownloadClient client, IndexConfiguration.ClientConfiguration configuration)> DownloadClients { get; set; }

        public IndexService(IServiceProvider services) : base(services)
        {
        }

        private static async Task Main(string[] args) => await Service<IndexService>.RunAsync(args, (configuration, services) =>
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

            services.AddFluffleThumbnail();

            services.AddTransient<ImageHasher>();
            services.AddTransient<Thumbnailer>();
            services.AddTransient<ThumbnailPublisher>();
            services.AddTransient<IndexPublisher>();
        });

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var configuration = Services.GetRequiredService<FluffleConfiguration>();
            Configuration = configuration.Get<IndexConfiguration>();

            var fluffleClient = Services.GetRequiredService<FluffleClient>();

            var e621Client = await new E621ClientFactory(configuration).CreateAsync(Configuration.E621.Interval);
            var furryNetworkClient = await new FurryNetworkClientFactory(configuration).CreateAsync(Configuration.FurryNetwork.Interval);
            var furAffinityClient = await new FurAffinityClientFactory(configuration).CreateAsync(Configuration.FurAffinity.Interval);
            var weasylClient = await new WeasylClientFactory(configuration).CreateAsync(Configuration.Weasyl.Interval);
            var twitterClient = await new TwitterDownloadClientFactory(configuration).CreateAsync(Configuration.Twitter.Interval);
            DownloadClients = new Dictionary<PlatformConstant, (DownloadClient, IndexConfiguration.ClientConfiguration)>
            {
                { PlatformConstant.E621, (new E621DownloadClient(e621Client), Configuration.E621) },
                { PlatformConstant.FurryNetwork, (new FurryNetworkDownloadClient(furryNetworkClient), Configuration.FurryNetwork) },
                { PlatformConstant.FurAffinity, (new FurAffinityDownloadClient(furAffinityClient, fluffleClient, Environment), Configuration.FurAffinity) },
                { PlatformConstant.Weasyl, (new WeasylDownloadClient(weasylClient), Configuration.Weasyl) },
                { PlatformConstant.Twitter , (new TwitterDownloadClient(twitterClient), Configuration.Twitter) }
            };

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (Environment.IsProduction())
            {
                var testRunner = Services.GetRequiredService<FluffleHashSelfTestRunner>();
                testRunner.Run();
            }

            var fluffleClient = Services.GetRequiredService<FluffleClient>();

            var manager = new ProducerConsumerManager<ChannelImage>(Services, 20);

            var platforms = await HttpResiliency.RunAsync(() => fluffleClient.GetPlatformsAsync());
            foreach (var platform in platforms)
            {
                var sourceConstant = (PlatformConstant)platform.Id;
                if (!DownloadClients.TryGetValue(sourceConstant, out var x))
                {
                    Log.Warning("There exists no download client for {platformName}.", platform.Name);
                    continue;
                }

                Log.Information("[{platformName}] Starting indexing...", platform.Name);
                manager.AddProducer(x.configuration.Threads, () => new ImageDownloader(fluffleClient, platform, x.client));
            }

            manager.AddConsumer<ImageHasher>(Configuration.ImageHasher.Threads, Configuration.ImageHasher.Buffer);
            manager.AddConsumer<Thumbnailer>(Configuration.Thumbnailer.Threads, Configuration.Thumbnailer.Buffer);
            manager.AddConsumer<ThumbnailPublisher>(Configuration.ThumbnailPublisher.Threads, Configuration.ThumbnailPublisher.Buffer);
            manager.AddFinalConsumer<IndexPublisher>(Configuration.IndexPublisher.Threads);

            await manager.RunAsync();
        }
    }
}

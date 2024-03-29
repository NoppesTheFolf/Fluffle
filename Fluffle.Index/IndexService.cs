﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Noppes.Fluffle.B2;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.E621Sync;
using Noppes.Fluffle.FurAffinitySync;
using Noppes.Fluffle.FurryNetworkSync;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Imaging.Tests;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.PerceptualHashing;
using Noppes.Fluffle.Service;
using Noppes.Fluffle.Thumbnail;
using Noppes.Fluffle.Twitter.Client;
using Noppes.Fluffle.Utils;
using Noppes.Fluffle.WeasylSync;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index;

public class IndexService : Service.Service
{
    private const string UserAgentApplicationName = "index";

    private IndexConfiguration Configuration { get; set; }
    private Dictionary<PlatformConstant, (DownloadClient client, IndexConfiguration.ClientConfiguration configuration)> DownloadClients { get; set; }

    public IndexService(IServiceProvider services) : base(services)
    {
    }

    private static async Task Main(string[] args) => await Service<IndexService>.RunAsync(args, "Index", (conf, services) =>
    {
        var mainConf = conf.Get<MainConfiguration>();
        var fluffleClient = new FluffleClient(mainConf.Url, mainConf.ApiKey);
        services.AddSingleton(fluffleClient);

        services.AddTwitterApiClient(conf);

        services.AddFluffleThumbnail();

        var fluffleHash = new FluffleHash();
        services.AddSingleton(fluffleHash);

        services.AddImagingTests(_ => Log.Information);

        var thumbConf = conf.Get<ThumbnailConfiguration>();
        var b2Conf = conf.Get<BackblazeB2Configuration>();
        var b2Client = new B2Client(b2Conf.ApplicationKeyId, b2Conf.ApplicationKey, thumbConf.BaseUrl);
        services.AddSingleton(b2Client);
        services.AddSingleton(new B2ThumbnailStorage(b2Client, thumbConf.Salt));

        services.AddTransient<ImageHasher>();
        services.AddTransient<Thumbnailer>();
        services.AddTransient<ThumbnailPublisher>();
        services.AddTransient<IndexPublisher>();
    });

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var configuration = Services.GetRequiredService<FluffleConfiguration>();
        Configuration = configuration.Get<IndexConfiguration>();

        var twitterApiClient = Services.GetRequiredService<ITwitterApiClient>();
        var e621Client = await new E621ClientFactory(configuration).CreateAsync(Configuration.E621.Interval, UserAgentApplicationName);
        var furryNetworkClient = await new FurryNetworkClientFactory(configuration).CreateAsync(Configuration.FurryNetwork.Interval, UserAgentApplicationName);
        var furAffinityClient = await new FurAffinityClientFactory(configuration).CreateAsync(Configuration.FurAffinity.Interval, UserAgentApplicationName);
        var weasylClient = await new WeasylClientFactory(configuration).CreateAsync(Configuration.Weasyl.Interval, UserAgentApplicationName);
        var deviantArtClient = await new BasicClientFactory(configuration).CreateAsync(Configuration.DeviantArt.Interval, UserAgentApplicationName);
        var inkbunnyClient = await new BasicClientFactory(configuration).CreateAsync(Configuration.Inkbunny.Interval, UserAgentApplicationName);
        DownloadClients = new Dictionary<PlatformConstant, (DownloadClient, IndexConfiguration.ClientConfiguration)>
        {
            { PlatformConstant.E621, (new E621DownloadClient(e621Client), Configuration.E621) },
            { PlatformConstant.FurAffinity, (new FurAffinityDownloadClient(furAffinityClient), Configuration.FurAffinity) },
            { PlatformConstant.Weasyl, (new WeasylDownloadClient(weasylClient), Configuration.Weasyl) },
            { PlatformConstant.Twitter , (new FuncDownloadClient(url => twitterApiClient.GetStreamAsync(url, false)), Configuration.Twitter) },
            { PlatformConstant.DeviantArt, (new BasicDownloadClient(deviantArtClient), Configuration.DeviantArt) },
            { PlatformConstant.Inkbunny, (new BasicDownloadClient(inkbunnyClient), Configuration.Inkbunny) }
        };

        try
        {
            await furryNetworkClient.SearchAsync();
            DownloadClients.Add(PlatformConstant.FurryNetwork, (new FurryNetworkDownloadClient(furryNetworkClient), Configuration.FurryNetwork));
        }
        catch (InvalidOperationException)
        {
            Log.Error("Furry Network credentials have expired.");
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteServiceAsync(CancellationToken stoppingToken)
    {
        if (Environment.IsProduction())
        {
            var testsExecutor = Services.GetRequiredService<IImagingTestsExecutor>();
            testsExecutor.Execute();
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

        await manager.RunAsync(stoppingToken);
    }
}

﻿using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Noppes.E621;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.E621Sync;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.TwitterSync.AnalyzeMedia;
using Noppes.Fluffle.TwitterSync.AnalyzeUsers;
using Noppes.Fluffle.TwitterSync.Database.Models;
using Noppes.Fluffle.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;

namespace Noppes.Fluffle.TwitterSync
{
    public partial class SyncClient : Service.Service<SyncClient>
    {
        private static async Task Main(string[] args) => await RunAsync(args, (conf, services) =>
        {
            // Add main API client
            var mainConfiguration = conf.Get<MainConfiguration>();
            var fluffleClient = new FluffleClient(mainConfiguration.Url, mainConfiguration.ApiKey);
            services.AddSingleton(fluffleClient);

            // Add Twitter API client
            var twitterConf = conf.Get<TwitterConfiguration>();
            var credentials = new ConsumerOnlyCredentials(twitterConf.ApiKey, twitterConf.ApiKeySecret, twitterConf.BearerToken);
            var client = new TwitterClient(credentials);
            client.Config.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;
            services.AddSingleton<ITwitterClient>(client);

            // Add client used for downloading images from Twitter
            var downloadClient = new TwitterDownloadClientFactory(conf).CreateAsync(200).Result;
            services.AddSingleton(downloadClient);

            // Add E621 API client
            var e621Client = new E621ClientFactory(conf).CreateAsync(E621Constants.RecommendedRequestIntervalInMilliseconds).Result;
            services.AddSingleton(e621Client);

            // Add prediction client
            var predictionConf = conf.Get<PredictionConfiguration>();
            var predictionClient = new PredictionClient(predictionConf.Url);
            services.AddSingleton<IPredictionClient>(predictionClient);

            // Add Fluffle reverse search client
            var reverseSearchClient = new ReverseSearchClient();
            services.AddSingleton<IReverseSearchClient>(reverseSearchClient);

            // Add database
            services.AddDbContext<TwitterContext>(options =>
            {
                // options.LogTo(Console.WriteLine);
                // options.EnableSensitiveDataLogging();
                options.UseNpgsql(conf.Get<TwitterDatabaseConfiguration>().ConnectionString);
            });

            // Configure user analyze consumers/producers
            services.AddTransient<UserSupplier>();
            services.AddSingleton<ImageRetriever<AnalyzeUserData>>();
            services.AddTransient<PredictClasses<AnalyzeUserData>>();
            services.AddTransient<ReverseSearch>();
            services.AddTransient<PredictIfArtist>();
            services.AddTransient<UpsertIfArtist>();

            // Configure media analyze consumers/producers
            services.AddTransient<MediaSupplier>();
            services.AddSingleton<ImageRetriever<AnalyzeMediaData>>();
            services.AddTransient<PredictClasses<AnalyzeMediaData>>();
            services.AddTransient<PredictIfFurryArt>();
            services.AddTransient<SubmitIfFurryArt>();
        });

        private readonly ITwitterClient _twitterClient;
        private readonly IE621Client _e621Client;

        public SyncClient(IServiceProvider services, ITwitterClient twitterClient, IE621Client e621Client) : base(services)
        {
            _twitterClient = twitterClient;
            _e621Client = e621Client;
        }

        private async Task ApplyMigrationsAsync()
        {
            using var scope = Services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<TwitterContext>();

            context.Database.SetCommandTimeout(30.Minutes());
            await context.Database.MigrateAsync();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ApplyMigrationsAsync();

            using var scope = Services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<TwitterContext>();

            await SyncE621ArtistsAsync(stoppingToken);
            await SyncTwitterArtists(stoppingToken);

            var taskOne = Task.Run(async () =>
            {
                var manager = new ProducerConsumerManager<AnalyzeUserData>(Services, 5);
                manager.AddProducer<UserSupplier>(1);
                manager.AddConsumer<ImageRetriever<AnalyzeUserData>>(1, 5);
                manager.AddConsumer<PredictClasses<AnalyzeUserData>>(1, 5);
                manager.AddConsumer<ReverseSearch>(4, 5);
                manager.AddConsumer<PredictIfArtist>(1, 5);
                manager.AddFinalConsumer<UpsertIfArtist>(1);

                await manager.RunAsync();
            }, stoppingToken);

            var taskTwo = Task.Run(async () =>
            {
                var manager = new ProducerConsumerManager<AnalyzeMediaData>(Services, 5);
                manager.AddProducer<MediaSupplier>(1);
                manager.AddConsumer<ImageRetriever<AnalyzeMediaData>>(1, 5);
                manager.AddConsumer<PredictClasses<AnalyzeMediaData>>(1, 5);
                manager.AddConsumer<PredictIfFurryArt>(1, 5);
                manager.AddFinalConsumer<SubmitIfFurryArt>(1);

                await manager.RunAsync();
            }, stoppingToken);

            var task = await Task.WhenAny(taskOne, taskTwo);
            if (task.Exception != null) throw task.Exception;

            // This were the accounts used to train some models on. Kept here for future use.
            var users = new Dictionary<string, bool>
            {
                // Artists
                { "WagnerSmut", true },
                { "Vesper_Art", true },
                { "WagnerMutt", true },
                { "TheHearthFox", true },
                { "_Neketo", true },
                { "PeyoteTheHyena", true },
                { "LockworkArt", true },
                { "silvixenart", true },
                { "KitaKettu", true },
                { "SushiSusii", true },
                { "zorryn_art", true },
                { "ButteredShep", true },
                { "Jacato_", true },
                { "OggyOsbourne", true },
                { "waywardmutt", true },
                { "rainingcoyotes", true },
                { "angiewolfartist", true },
                { "swishchee", true },
                { "thanshuhai", true },
                { "TheDiyemi", true },
                { "wereshiba", true },
                { "MushroomHuie", true },
                { "Tsaiwolf", true },
                { "JibKodi", true },
                { "SparksFurArt", true },
                { "MVPDog", true },
                { "BattyTangFang", true },
                { "PastelDawg", true },
                // Anime artists
                { "horikoshiko", false },
                { "avogado6", false },
                { "y_o_m_y_o_m", false },
                { "houshoumarine", false },
                { "SakimiChanArt", false },
                { "cutesexyrobutts", false },
                { "udon0531", false },
                { "esasi8794", false },
                { "hirame_sa", false },
                { "ThiccWithaQ", false },
                { "bkub_comic", false },
                { "AfrobullArt", false },
                // Fursuits
                { "JurassiCats", false },
                { "FoxiesCreations", false },
                { "tallfuzzball", false },
                { "Mojo_Coyote", false },
                { "SkyeCabbit", false },
                { "Elliotfolf", false },
                { "TemplaCreations", false },
                // Accounts without any art
                { "ServalEveryHr", false },
                { "hourlyfoxes", false },
                { "nature_org", false },
                { "RedPandaEveryHr", false },
                { "CNN", false },
                { "guardian", false },
                { "IwriteOK", false },
                // Bots that post art from various sources
                { "YiffyOttBot", false },
                { "YiffMe_Bot", false },
                { "femboifrost", false },
                { "furrygaysexblog", false },
                { "YiffFemboy", false }
            };
        }
    }
}

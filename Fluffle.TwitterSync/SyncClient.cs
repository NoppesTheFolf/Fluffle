using Dasync.Collections;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Noppes.E621;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.E621Sync;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.TwitterSync.AnalyzeMedia;
using Noppes.Fluffle.TwitterSync.AnalyzeUsers;
using Noppes.Fluffle.TwitterSync.Database.Models;
using Noppes.Fluffle.TwitterSync.RefreshTimeline;
using Noppes.Fluffle.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            // Add sync configuration
            var syncConf = conf.Get<TwitterSyncConfiguration>();
            services.AddSingleton(syncConf);

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

            // Add efficient tweet retriever
            services.AddSingleton<TweetRetriever>();

            // Add client used for downloading images from Twitter
            var downloadClient = new TwitterDownloadClientFactory(conf).CreateAsync(syncConf.DownloadInterval).Result;
            services.AddSingleton(downloadClient);

            // Add E621 API client
            var e621Client = new E621ClientFactory(conf).CreateAsync(E621Constants.RecommendedRequestIntervalInMilliseconds).Result;
            services.AddSingleton(e621Client);

            // Add prediction client
            var predictionConf = conf.Get<PredictionConfiguration>();
            var predictionClient = new PredictionClient(predictionConf.Url, predictionConf.ApiKey, predictionConf.ClassifyDegreeOfParallelism);
            services.AddSingleton<IPredictionClient>(predictionClient);

            // Add Fluffle reverse search client
            var reverseSearchClient = new ReverseSearchClient();
            services.AddSingleton<IReverseSearchClient>(reverseSearchClient);

            // Add database
            services.AddDatabase<TwitterContext, TwitterDatabaseConfiguration>(conf);

            // Configure user analyze consumers/producers
            services.AddTransient<NewUserSupplier>();
            services.AddSingleton<ImageRetriever<AnalyzeUserData>>();
            services.AddTransient<PredictClasses<AnalyzeUserData>>();
            services.AddTransient<ReverseSearch>();
            services.AddTransient<PredictIfArtist>();
            services.AddTransient<FillMissingFromTimelineIfArtist<AnalyzeUserData>>();
            services.AddTransient<UpsertIfArtist<AnalyzeUserData>>();

            // Configure timeline refresh consumers/producers
            services.AddTransient<RefreshUserSupplier>();
            services.AddTransient<FillMissingFromTimelineIfArtist<RefreshTimelineData>>();
            services.AddTransient<UpsertIfArtist<RefreshTimelineData>>();

            // Configure media analyze consumers/producers
            services.AddTransient<MediaSupplier>();
            services.AddSingleton<ImageRetriever<AnalyzeMediaData>>();
            services.AddTransient<PredictClasses<AnalyzeMediaData>>();
            services.AddTransient<PredictIfFurryArt>();
            services.AddTransient<SubmitIfFurryArt>();
        });

        private readonly ITwitterClient _twitterClient;
        private readonly IE621Client _e621Client;
        private readonly FluffleClient _fluffleClient;
        private readonly TweetRetriever _tweetRetriever;

        public SyncClient(IServiceProvider services, ITwitterClient twitterClient, IE621Client e621Client, FluffleClient fluffleClient, TweetRetriever tweetRetriever) : base(services)
        {
            _twitterClient = twitterClient;
            _e621Client = e621Client;
            _fluffleClient = fluffleClient;
            _tweetRetriever = tweetRetriever;
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

            var taskOne = Task.Run(async () =>
            {
                while (true)
                {
                    await SyncE621ArtistsAsync(stoppingToken);
                    await SyncTwitterArtists(stoppingToken);

                    await SyncOtherSourcesAsync();

                    await Task.Delay(1.Hours());
                }
            }, stoppingToken);

            var taskTwo = Task.Run(async () =>
            {
                var manager = new ProducerConsumerManager<AnalyzeUserData>(Services, 5);
                manager.AddProducer<NewUserSupplier>(1);
                manager.AddConsumer<ImageRetriever<AnalyzeUserData>>(1, 5);
                manager.AddConsumer<PredictClasses<AnalyzeUserData>>(1, 5);
                manager.AddConsumer<ReverseSearch>(4, 5);
                manager.AddConsumer<PredictIfArtist>(1, 5);
                manager.AddConsumer<FillMissingFromTimelineIfArtist<AnalyzeUserData>>(5, 5);
                manager.AddFinalConsumer<UpsertIfArtist<AnalyzeUserData>>(1);

                await manager.RunAsync();
            }, stoppingToken);

            var taskThree = Task.Run(async () =>
            {
                var manager = new ProducerConsumerManager<AnalyzeMediaData>(Services, 5);
                manager.AddProducer<MediaSupplier>(1);
                manager.AddConsumer<ImageRetriever<AnalyzeMediaData>>(1, 5);
                manager.AddConsumer<PredictClasses<AnalyzeMediaData>>(1, 5);
                manager.AddConsumer<PredictIfFurryArt>(1, 5);
                manager.AddFinalConsumer<SubmitIfFurryArt>(1);

                await manager.RunAsync();
            }, stoppingToken);

            var taskFour = Task.Run(async () =>
            {
                var manager = new ProducerConsumerManager<RefreshTimelineData>(Services, 5);
                manager.AddProducer<RefreshUserSupplier>(1);
                manager.AddConsumer<FillMissingFromTimelineIfArtist<RefreshTimelineData>>(95, 5);
                manager.AddFinalConsumer<UpsertIfArtist<RefreshTimelineData>>(1);

                await manager.RunAsync();
            }, stoppingToken);

            var taskFive = Task.Run(async () =>
            {
                var tweetRetriever = Services.GetRequiredService<TweetRetriever>();
                await tweetRetriever.RunAsync();
            }, stoppingToken);

            var task = await Task.WhenAny(taskOne, taskTwo, taskThree, taskFour, taskFive);
            if (task.Exception != null) throw task.Exception;
            throw new InvalidOperationException("One of the tasks exited. This should not be possible.");

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
                { "JurassiCats", true },
                { "tallfuzzball", true },
                { "Mojo_Coyote", true },
                { "SkyeCabbit", true },
                { "Elliotfolf", true },
                { "TemplaCreations", true },
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
                { "WorldOfYiff", false },
                { "femboifrost", false },
                { "furrygaysexblog", false },
                { "YiffFemboy", false },
                { "redditfurryporn", false }
            };

            var downloadClient = Services.GetRequiredService<ITwitterDownloadClient>();
            var twitterClient = Services.GetRequiredService<ITwitterClient>();
            var tweetRetriever = Services.GetRequiredService<TweetRetriever>();
            var predictionClient = Services.GetRequiredService<IPredictionClient>();
            var reverseSearchClient = Services.GetRequiredService<IReverseSearchClient>();
            await users.ParallelForEachAsync(async (kv) =>
            {
                var (username, isFurryArtist) = kv;

                var destLocation = Path.Join("./data", username + ".json");
                if (File.Exists(destLocation))
                    return;

                var user = await twitterClient.Users.GetUserAsync(username);
                var timeline = await TimelineCollection.CreateAsync(twitterClient, tweetRetriever, user);
                var images = timeline
                    .Where(t => t.CreatedBy.IdStr == user.IdStr && t.Type() == TweetType.Post)
                    .Select(t => (tweet: t, media: t.Media.FirstOrDefault(m => m.MediaType() == MediaTypeConstant.Image)))
                    .Where(x => x.media != null)
                    .OrderByDescending(x => x.tweet.CreatedAt)
                    .Take(NewUserSupplier.ImagesPopularFactor * NewUserSupplier.ImagesBatchSize)
                    .OrderByDescending(x => x.tweet.FavoriteCount)
                    .Take(NewUserSupplier.ImagesBatchSize)
                    .ToList();

                var streams = new List<Func<Stream>>();
                foreach (var image in images)
                {
                    var stream = await downloadClient.GetStreamAsync(image.media.MediaURLHttps);

                    streams.Add(() =>
                    {
                        stream.Position = 0;

                        var copy = new MemoryStream();
                        stream.CopyTo(copy);
                        copy.Position = 0;

                        return copy;
                    });
                }

                var classes = await predictionClient.ClassifyAsync(streams);
                var isFurryArt = await predictionClient.IsFurryArtAsync(classes);

                var artistIds = new List<int[]>();
                foreach (var stream in streams)
                {
                    // Reverse search on Fluffle using only e621 as a source as that will always have the artist attached
                    var searchResult = await reverseSearchClient.ReverseSearchAsync(stream, true, 8, FlufflePlatform.E621);
                    var bestMatch = searchResult.Results
                        .Where(r => r.Match != FluffleMatch.Unlikely)
                        .OrderByDescending(r => r.Match)
                        .ThenByDescending(r => r.Score)
                        .FirstOrDefault();

                    var ids = bestMatch == null || bestMatch.Credits.Count > 1
                        ? Array.Empty<int>()
                        : bestMatch.Credits.Select(c => c.Id).ToArray();

                    artistIds.Add(ids);
                }

                var result = new DataRetrievalResult
                {
                    Username = user.ScreenName,
                    IsFurryArtist = isFurryArtist,
                    Classes = classes,
                    ArtistIds = artistIds.SelectMany(a => a).Distinct().ToList()
                };
                await File.WriteAllTextAsync(destLocation, JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }));
            }, 8);
        }
    }

    public class DataRetrievalResult
    {
        public string Username { get; set; }

        public bool IsFurryArtist { get; set; }

        public ICollection<IDictionary<bool, double>> Classes { get; set; }

        public ICollection<int> ArtistIds { get; set; }
    }
}

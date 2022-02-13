using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.B2;
using Noppes.Fluffle.Bot.Controllers;
using Noppes.Fluffle.Bot.Database;
using Noppes.Fluffle.Bot.Interceptors;
using Noppes.Fluffle.Bot.Routing;
using Noppes.Fluffle.Bot.Utils;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Service;
using Noppes.Fluffle.Thumbnail;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Noppes.Fluffle.Bot
{
    public class BucketCollection
    {
        public B2Bucket Index { get; set; }

        public B2Bucket Thumbnail { get; set; }

        public BucketCollection(B2Bucket index, B2Bucket thumbnail)
        {
            Index = index;
            Thumbnail = thumbnail;
        }
    }

    public class UploadManagerCollection
    {
        public B2UploadManager Index { get; set; }

        public B2UploadManager Thumbnail { get; set; }

        public UploadManagerCollection(B2UploadManager index, B2UploadManager thumbnail)
        {
            Index = index;
            Thumbnail = thumbnail;
        }
    }

    public class Program : Service.Service
    {
        public Program(IServiceProvider services) : base(services)
        {
        }

        private static async Task Main(string[] args) => await Service<Program>.RunAsync(args, (configuration, services) =>
        {
            services.AddFluffleThumbnail();

            var botConf = configuration.Get<BotConfiguration>();
            services.AddSingleton(botConf);

            var indexB2Client = new B2Client(botConf.IndexBackblazeB2.ApplicationKeyId, botConf.IndexBackblazeB2.ApplicationKey);
            var indexBucket = indexB2Client.GetBucketAsync().Result;

            var thumbnailB2Client = new B2Client(botConf.ThumbnailBackblazeB2.ApplicationKeyId, botConf.ThumbnailBackblazeB2.ApplicationKey);
            var thumbnailBucket = thumbnailB2Client.GetBucketAsync().Result;

            services.AddSingleton(new BucketCollection(indexBucket, thumbnailBucket));

            var b2IndexUploadManager = new B2UploadManager(botConf.IndexBackblazeB2.Workers, indexBucket);
            var b2ThumbnailUploaderManager = new B2UploadManager(botConf.ThumbnailBackblazeB2.Workers, thumbnailBucket);
            services.AddSingleton(new UploadManagerCollection(b2IndexUploadManager, b2ThumbnailUploaderManager));

            services.AddSingleton<ITelegramRepository<CallbackContext, string>, CallbackContextRepository>();
            services.AddSingleton<CallbackManager>();

            services.AddSingleton<ITelegramRepository<InputContext, long>, InputContextRepository>();
            services.AddSingleton<InputManager>();

            var fluffleClient = new FluffleClient();
            services.AddSingleton(fluffleClient);
            services.AddSingleton(new ReverseSearchScheduler(botConf.ReverseSearch.Workers, fluffleClient));
            services.AddSingleton<ReverseSearchRequestLimiter>();

            services.AddSingleton<MediaGroupTracker>();
            services.AddSingleton<MediaGroupHandler>();

            var context = new BotContext(botConf.MongoConnectionString, botConf.MongoDatabase);
            services.AddSingleton(context);
            services.AddSingleton(context.CallbackContexts);
            services.AddSingleton(context.InputContexts);

            // Configure rate limiter to use values defined in the config
            RateLimiter.Initialize(botConf.TelegramGlobalBurstLimit, botConf.TelegramGlobalBurstInterval, botConf.TelegramGroupBurstLimit, botConf.TelegramGroupBurstInterval);

            var botClient = new TelegramBotClient(botConf.TelegramToken);
            services.AddSingleton<ITelegramBotClient>(botClient);

            services.AddSingleton<TelegramRouter>();
            services.AddTransient<TelegramRouterWorker>();

            services.AddSingleton<ChatRegisterInterceptor>();

            services.AddTransient<ChatTrackingController>();
            services.AddTransient<SettingsMenuController>();
            services.AddTransient<ReverseSearchController>();
            services.AddTransient<RateLimitController>();
        });

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Template.CompileAsync();

            var router = Services.GetRequiredService<TelegramRouter>();

            router.RegisterInterceptor<ChatRegisterInterceptor>();

            router.CommandHandlers.Add("help", new FuncUpdateHandler(Template.Help));
            router.CommandHandlers.Add("ihasfoundbug", new FuncUpdateHandler(Template.IHasFoundBug));

            router.RegisterController<ChatTrackingController>();
            router.RegisterController<SettingsMenuController>();
            router.RegisterController<ReverseSearchController>();
            router.RegisterController<RateLimitController>();

            await router.RunAsync(cancellationToken);
        }
    }
}

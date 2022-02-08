using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Bot.Controllers;
using Noppes.Fluffle.Bot.Database;
using Noppes.Fluffle.Bot.Interceptors;
using Noppes.Fluffle.Bot.Routing;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Service;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Noppes.Fluffle.Bot
{
    public class Program : Service.Service
    {
        public Program(IServiceProvider services) : base(services)
        {
        }

        private static async Task Main(string[] args) => await Service<Program>.RunAsync(args, (configuration, services) =>
        {
            var botConf = configuration.Get<BotConfiguration>();
            services.AddSingleton(botConf);

            services.AddSingleton<ITelegramRepository<CallbackContext, string>, CallbackContextRepository>();
            services.AddSingleton<CallbackManager>();

            services.AddSingleton<ITelegramRepository<InputContext, long>, InputContextRepository>();
            services.AddSingleton<InputManager>();

            var fluffleClient = new FluffleClient();
            services.AddSingleton(fluffleClient);
            services.AddSingleton(new ReverseSearchScheduler(botConf.TelegramReverseSearchWorkersCount, fluffleClient));
            services.AddSingleton<ReverseSearchRequestLimiter>();

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

            await router.RunAsync(cancellationToken);
        }
    }
}

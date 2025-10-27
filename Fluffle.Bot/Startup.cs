using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Noppes.Fluffle.Api;
using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.Bot.Controllers;
using Noppes.Fluffle.Bot.Database;
using Noppes.Fluffle.Bot.Interceptors;
using Noppes.Fluffle.Bot.Routing;
using Noppes.Fluffle.Bot.Utils;
using Noppes.Fluffle.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Noppes.Fluffle.Bot;

public class Startup : ApiStartup<Startup>
{
    private const string UserAgentApplicationName = "telegram-bot";

    protected override string ApplicationName => "TelegramBot";

    protected override bool EnableAccessControl => false;

    public override void AdditionalConfigureServices(IServiceCollection services)
    {
        var botConf = Configuration.Get<BotConfiguration>();
        services.AddSingleton(botConf);

        services.AddSingleton<ITelegramRepository<CallbackContext, string>, CallbackContextRepository>();
        services.AddSingleton<CallbackManager>();

        services.AddSingleton<ITelegramRepository<InputContext, long>, InputContextRepository>();
        services.AddSingleton<InputManager>();

        var fluffleClient = new FluffleClient(UserAgentApplicationName);
        services.AddSingleton(fluffleClient);
        services.AddSingleton(new ReverseSearchScheduler(botConf.ReverseSearch.Workers, fluffleClient));
        services.AddSingleton<ReverseSearchRequestLimiter>();

        services.AddSingleton<MessageCleaner>();

        var context = new BotContext(botConf.MongoConnectionString, botConf.MongoDatabase);
        services.AddSingleton(context);
        services.AddSingleton(context.CallbackContexts);
        services.AddSingleton(context.InputContexts);

        // Configure rate limiter to use values defined in the config
        RateLimiter.Initialize(botConf.TelegramGlobalBurstLimit, botConf.TelegramGlobalBurstInterval, botConf.TelegramGroupBurstLimit, botConf.TelegramGroupBurstInterval);

        var botClient = new TelegramBotClient(botConf.TelegramToken);
        services.AddSingleton<ITelegramBotClient>(botClient);
        services.AddHostedService<ConfigureWebhook>();
        services.AddSingleton<TaskAwaiter<TelegramRouter>>();

        services.AddSingleton<TelegramRouter>();
        services.AddTransient<TelegramRouterWorker>();

        services.AddSingleton<ChatRegisterInterceptor>();

        services.AddTransient<ChatTrackingController>();
        services.AddTransient<SettingsMenuController>();
        services.AddTransient<ReverseSearchController>();
        services.AddTransient<RateLimitController>();
    }

    public override void AfterConfigure(IApplicationBuilder app, IWebHostEnvironment env, ServiceBuilder serviceBuilder)
    {
        var router = app.ApplicationServices.GetRequiredService<TelegramRouter>();

        router.RegisterInterceptor<ChatRegisterInterceptor>();

        const string helpText = """
                                I am a bot that can reverse search furry art\. I'll try to find the sources of any images you throw at me\! I can also help out in channels and groups chats, check out Fluffle its [bot documentation](https://fluffle.xyz/tools/telegram-bot/) if you are interested in that\.
                                
                                *Configuration*
                                [The website](http://fluffle.xyz/tools/telegram-bot/#configuration) contains examples and more details about how you can configure the bot\.
                                
                                /setformat \- Set if sources should be presented using the inline keyboard or text/captions\.
                                /setinlinekeyboardformat \- When the inline keyboard format is used, configure the bot to use a specific inline keyboard format of your liking\.
                                /settextformat \- When the text format is used, configure the bot to use a specific text format of your liking\.
                                /settextseparator \- Configure the bot to use a separator character of your liking\. Applied when using the platform names text format\.
                                
                                *Rate limits*
                                The bot makes use of rate limiting and prioritization to prevent abuse and guarantee service consistently to everyone\. Per chat, per 24\-hour period, a chat is allowed to make 400 reverse search requests\. Exceed this, and the bot will start ignoring any images sent\.
                                
                                /ratelimits \- See the reverse search consumption of your chats\.
                                
                                *Miscellaneous*
                                /ihasfoundbug \- Found a bug? Get information on how to report it\.
                                """;
        router.CommandHandlers.Add("help", new FuncUpdateHandler(_ => helpText));

        const string iHasFoundBugText = """
                                        You can report any issues you have with the bot to @NoppesTheFolf\. If possible, please try to describe the problem you are experiencing in a way that is reproducible/replicable\.
                                        """;
        router.CommandHandlers.Add("ihasfoundbug", new FuncUpdateHandler(_ => iHasFoundBugText));

        router.RegisterController<ChatTrackingController>();
        router.RegisterController<SettingsMenuController>();
        router.RegisterController<ReverseSearchController>();
        router.RegisterController<RateLimitController>();

        var conf = Configuration.Get<BotConfiguration>();
        serviceBuilder.AddSingleton<MessageCleaner>(TimeSpan.FromMinutes(conf.MessageCleaner.Interval));
    }
}

public class ConfigureWebhook : IHostedService
{
    private readonly ITelegramBotClient _botClient;
    private readonly TaskAwaiter<TelegramRouter> _taskAwaiter;
    private readonly BotConfiguration _botConfiguration;
    private readonly ILogger<ConfigureWebhook> _logger;

    public ConfigureWebhook(ITelegramBotClient botClient, TaskAwaiter<TelegramRouter> taskAwaiter, BotConfiguration botConfiguration, ILogger<ConfigureWebhook> logger)
    {
        _botClient = botClient;
        _taskAwaiter = taskAwaiter;
        _botConfiguration = botConfiguration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Setup webhook on app startup
        _logger.LogInformation("Setting webhook...");

        var url = $"https://{_botConfiguration.TelegramHost}/{_botConfiguration.TelegramToken}";
        await _botClient.SetWebhookAsync(url, cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Wait for all requests to the bot to be handled
        _logger.LogInformation("Waiting for all requests to be handled...");
        _taskAwaiter.CancellationTokenSource.Cancel();
        await _taskAwaiter.WaitTillAllCompleted();

        // Remove webhook upon app shutdown
        _logger.LogInformation("Removing webhook...");

        // Do not use cancellation here. Waiting for all tasks to complete might take a while
        // and therefore the provided cancellation token might get set to cancelled.
        await _botClient.DeleteWebhookAsync();
    }
}

public class WebhookController : ControllerBase
{
    private readonly BotConfiguration _botConfiguration;
    private readonly TelegramRouter _router;
    private readonly TaskAwaiter<TelegramRouter> _taskAwaiter;

    public WebhookController(BotConfiguration botConfiguration, TelegramRouter router, TaskAwaiter<TelegramRouter> taskAwaiter)
    {
        _botConfiguration = botConfiguration;
        _router = router;
        _taskAwaiter = taskAwaiter;
    }

    [HttpPost("/{token}")]
    public async Task<IActionResult> ReceiveUpdate(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest();

        if (token != _botConfiguration.TelegramToken)
            return Unauthorized();

        using var streamReader = new StreamReader(Request.Body);
        var json = await streamReader.ReadToEndAsync();
        var update = JsonConvert.DeserializeObject<Update>(json);

        _taskAwaiter.Add(Task.Run(() => _router.HandleUpdateAsync(update)));

        return Ok();
    }
}

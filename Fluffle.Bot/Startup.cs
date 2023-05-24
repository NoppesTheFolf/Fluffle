using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Noppes.Fluffle.Api;
using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.B2;
using Noppes.Fluffle.Bot.Controllers;
using Noppes.Fluffle.Bot.Database;
using Noppes.Fluffle.Bot.Interceptors;
using Noppes.Fluffle.Bot.Routing;
using Noppes.Fluffle.Bot.Utils;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Thumbnail;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Noppes.Fluffle.Bot;

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

public class Startup : ApiStartup<Startup>
{
    private const string UserAgentApplicationName = "telegram-bot";

    protected override string ApplicationName => "TelegramBot";

    protected override bool EnableAccessControl => false;

    public override void AdditionalConfigureServices(IServiceCollection services)
    {
        services.AddFluffleThumbnail();

        var botConf = Configuration.Get<BotConfiguration>();
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

        var fluffleClient = new FluffleClient(UserAgentApplicationName);
        services.AddSingleton(fluffleClient);
        services.AddSingleton(new ReverseSearchScheduler(botConf.ReverseSearch.Workers, fluffleClient));
        services.AddSingleton<ReverseSearchRequestLimiter>();

        services.AddSingleton<MessageCleaner>();

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
        Template.CompileAsync().Wait();

        var router = app.ApplicationServices.GetRequiredService<TelegramRouter>();

        router.RegisterInterceptor<ChatRegisterInterceptor>();

        router.CommandHandlers.Add("help", new FuncUpdateHandler(Template.Help));
        router.CommandHandlers.Add("ihasfoundbug", new FuncUpdateHandler(Template.IHasFoundBug));

        router.RegisterController<ChatTrackingController>();
        router.RegisterController<SettingsMenuController>();
        router.RegisterController<ReverseSearchController>();
        router.RegisterController<RateLimitController>();

        var conf = Configuration.Get<BotConfiguration>();
        serviceBuilder.AddSingleton<MessageCleaner>(conf.MessageCleaner.Interval.Minutes());
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

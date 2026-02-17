using Fluffle.TelegramBot;
using Fluffle.TelegramBot.Controllers;
using Fluffle.TelegramBot.Database;
using Fluffle.TelegramBot.Interceptors;
using Fluffle.TelegramBot.ReverseSearch;
using Fluffle.TelegramBot.ReverseSearch.Api;
using Fluffle.TelegramBot.Routing;
using Fluffle.TelegramBot.Services;
using Fluffle.TelegramBot.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using IPNetwork = System.Net.IPNetwork;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddOptions<BotConfiguration>()
    .BindConfiguration("Bot")
    .ValidateDataAnnotations().ValidateOnStart();

services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor;
    options.KnownIPNetworks.Add(IPNetwork.Parse("0.0.0.0/0"));
    options.KnownIPNetworks.Add(IPNetwork.Parse("::/0"));
});

services.AddSingleton<ITelegramRepository<CallbackContext, string>, CallbackContextRepository>();
services.AddSingleton<CallbackManager>();

services.AddSingleton<ITelegramRepository<InputContext, long>, InputContextRepository>();
services.AddSingleton<InputManager>();

services.AddHttpClient(nameof(FluffleApiClient), client =>
{
    client.BaseAddress = new Uri("https://api.fluffle.xyz");
    client.DefaultRequestHeaders.Add("User-Agent", "fluffle.xyz/telegram-bot");
});
services.AddSingleton<FluffleApiClient>();

services.AddSingleton<ReverseSearchScheduler>();
services.AddSingleton<ReverseSearchRequestLimiter>();

services.AddSingleton<MessageCleanerService>();

services.AddSingleton<BotContext>();

services.AddSingleton<ITelegramBotClient>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<BotConfiguration>>();
    return new TelegramBotClient(options.Value.TelegramToken);
});
services.AddHostedService<ConfigureWebhookService>();
services.AddSingleton<TaskAwaiter<TelegramRouter>>();

services.AddSingleton<TelegramRouter>();
services.AddTransient<TelegramRouterWorker>();

services.AddSingleton<ChatRegisterInterceptor>();

services.AddTransient<ChatTrackingController>();
services.AddTransient<SettingsMenuController>();
services.AddTransient<ReverseSearchController>();
services.AddTransient<RateLimitController>();

var app = builder.Build();

app.UseForwardedHeaders();

app.MapPost("/{token}", async (
    string token,
    HttpRequest request,
    IOptions<BotConfiguration> options,
    TelegramRouter router,
    TaskAwaiter<TelegramRouter> taskAwaiter) =>
{
    if (string.IsNullOrWhiteSpace(token))
        return Results.BadRequest();

    if (token != options.Value.TelegramToken)
        return Results.Unauthorized();

    using var streamReader = new StreamReader(request.Body);
    var json = await streamReader.ReadToEndAsync();
    var update = JsonConvert.DeserializeObject<Update>(json);

    taskAwaiter.Add(Task.Run(() => router.HandleUpdateAsync(update)));

    return Results.Ok();
});

var router = app.Services.GetRequiredService<TelegramRouter>();

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

app.Run();

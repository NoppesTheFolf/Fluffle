using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Noppes.Fluffle.Bot.Routing;
using Noppes.Fluffle.Bot.Utils;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Noppes.Fluffle.Bot.Services;

public class ConfigureWebhookService : IHostedService
{
    private readonly ITelegramBotClient _botClient;
    private readonly TaskAwaiter<TelegramRouter> _taskAwaiter;
    private readonly IOptions<BotConfiguration> _options;
    private readonly ILogger<ConfigureWebhookService> _logger;

    public ConfigureWebhookService(ITelegramBotClient botClient, TaskAwaiter<TelegramRouter> taskAwaiter,
        IOptions<BotConfiguration> options, ILogger<ConfigureWebhookService> logger)
    {
        _botClient = botClient;
        _taskAwaiter = taskAwaiter;
        _options = options;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Setup webhook on app startup
        _logger.LogInformation("Setting webhook...");

        var url = $"https://{_options.Value.TelegramHost}/{_options.Value.TelegramToken}";
        await _botClient.SetWebhookAsync(url, cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Wait for all requests to the bot to be handled
        _logger.LogInformation("Waiting for all requests to be handled...");
        await _taskAwaiter.CancellationTokenSource.CancelAsync();
        await _taskAwaiter.WaitTillAllCompleted();

        // Remove webhook upon app shutdown
        _logger.LogInformation("Removing webhook...");

        // Do not use cancellation here. Waiting for all tasks to complete might take a while
        // and therefore the provided cancellation token might get set to cancelled.
        await _botClient.DeleteWebhookAsync(cancellationToken: CancellationToken.None);
    }
}

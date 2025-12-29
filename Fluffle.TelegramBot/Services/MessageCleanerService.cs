using System;
using System.Threading;
using System.Threading.Tasks;
using Fluffle.TelegramBot.Database;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fluffle.TelegramBot.Services;

public class MessageCleanerService : BackgroundService
{
    private readonly IOptions<BotConfiguration> _options;
    private readonly BotContext _botContext;
    private readonly ILogger<MessageCleanerService> _logger;

    public MessageCleanerService(IOptions<BotConfiguration> options, BotContext botContext, ILogger<MessageCleanerService> logger)
    {
        _options = options;
        _botContext = botContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var expirationDate = DateTime.UtcNow.Subtract(TimeSpan.FromHours(_options.Value.MessageCleaner.ExpirationTime));

                var removedCount = await _botContext.Messages.DeleteManyAsync(x => x.When < expirationDate);
                _logger.LogInformation("Deleted {count} old messages from the database.", removedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while cleaning up old messages from the database.");
            }

            var waitInterval = TimeSpan.FromMinutes(_options.Value.MessageCleaner.Interval);
            _logger.LogInformation("Waiting for {interval} minutes before the next messages cleanup.", waitInterval);
            await Task.Delay(waitInterval, stoppingToken);
        }
    }
}

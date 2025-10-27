using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.Bot.Database;
using Noppes.Fluffle.Configuration;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Bot;

public class MessageCleaner : IService
{
    private readonly BotConfiguration _botConfiguration;
    private readonly BotContext _botContext;
    private readonly ILogger<MessageCleaner> _logger;

    public MessageCleaner(BotConfiguration botConfiguration, BotContext botContext, ILogger<MessageCleaner> logger)
    {
        _botConfiguration = botConfiguration;
        _botContext = botContext;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        var expirationDate = DateTime.UtcNow.Subtract(TimeSpan.FromHours(_botConfiguration.MessageCleaner.ExpirationTime));

        var removedCount = await _botContext.Messages.DeleteManyAsync(x => x.When < expirationDate);
        _logger.LogInformation("Deleted {count} old messages from the database.", removedCount);
    }
}

using System;
using System.Text;
using System.Threading.Tasks;
using Fluffle.TelegramBot.Database;
using Fluffle.TelegramBot.ReverseSearch;
using Fluffle.TelegramBot.Routing;
using Fluffle.TelegramBot.Utils;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Fluffle.TelegramBot.Controllers;

public class RateLimitController
{
    private const int NumberOfBars = 8;

    private readonly IOptions<BotConfiguration> _options;
    private readonly ReverseSearchRequestLimiter _historyTracker;
    private readonly BotContext _context;
    private readonly ITelegramBotClient _botClient;

    public RateLimitController(IOptions<BotConfiguration> options, ReverseSearchRequestLimiter historyTracker, BotContext context, ITelegramBotClient botClient)
    {
        _options = options;
        _historyTracker = historyTracker;
        _context = context;
        _botClient = botClient;
    }

    [Command("ratelimits")]
    public async Task RateLimits(Message message)
    {
        var chats = await _context.Chats.GetOwnedChatsAsync(message.From!.Id);

        var builder = new StringBuilder();
        foreach (var (title, id) in chats)
        {
            var limitPerChat = _options.Value.ReverseSearch.RateLimiter.Count;

            // Add chat name
            builder.Append($"_{Markdown.Escape(title, ParseMode.MarkdownV2)}_\n");

            // Add used / total information
            var count = await _historyTracker.CountAsync(id);
            count = count > limitPerChat ? limitPerChat : count;
            builder.Append($"{count}/{limitPerChat} ");

            // Add progress bar
            var barsTaken = (int)Math.Round(count / (double)limitPerChat * NumberOfBars);
            builder.Append(new string('▆', barsTaken));
            builder.Append(new string('▁', NumberOfBars - barsTaken));

            // Add the percentage
            var percentage = (int)Math.Round(count / (double)limitPerChat * 100);
            builder.Append($" {percentage}%\n\n");
        }

        await RateLimiter.RunAsync(message.Chat, () => _botClient.SendTextMessageAsync(message.Chat.Id, builder.ToString(), ParseMode.MarkdownV2));
    }
}

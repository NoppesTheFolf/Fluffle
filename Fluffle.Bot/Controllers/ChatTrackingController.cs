using Noppes.Fluffle.Bot.Database;
using Noppes.Fluffle.Bot.Routing;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Noppes.Fluffle.Bot.Controllers;

internal class ChatTrackingController
{
    private readonly ITelegramBotClient _botClient;
    private readonly BotContext _context;

    public ChatTrackingController(ITelegramBotClient botClient, BotContext context)
    {
        _botClient = botClient;
        _context = context;
    }

    [Command("start")]
    public async Task Start(Message message)
    {
        if (message.Chat.Type != ChatType.Private)
            return;

        await _context.Chats.UpsertAsync(message.Chat, true, message.From!.Id, null);

        const string startText = """
                                 I am a bot that can reverse search furry art\. I'll try to find the sources of any images you throw at me\! I can also help out in channels and groups chats, check out Fluffle its [bot documentation](https://fluffle\.xyz/tools/telegram-bot/) if you are interested in that\.
                                 
                                 See /help to see which commands are available and such\.
                                 """;
        await RateLimiter.RunAsync(message.Chat, () => _botClient.SendTextMessageAsync(message.From.Id, startText, ParseMode.MarkdownV2));
    }

    [Update(UpdateType.MyChatMember)]
    public async Task HandleChatMember(ChatMemberUpdated chatMemberUpdated)
    {
        var isActive = chatMemberUpdated.NewChatMember is ChatMemberAdministrator;
        var chat = isActive ? await _botClient.GetChatAsync(chatMemberUpdated.Chat.Id) : chatMemberUpdated.Chat;

        User owner = null;
        if (isActive)
            owner = await _botClient.GetChatOwnerAsync(chat.Id);

        await _context.Chats.UpsertAsync(chat, isActive, owner?.Id, chatMemberUpdated.NewChatMember);
    }
}

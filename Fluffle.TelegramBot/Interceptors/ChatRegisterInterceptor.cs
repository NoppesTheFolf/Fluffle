using System.Collections.Generic;
using System.Threading.Tasks;
using Fluffle.TelegramBot.Database;
using Fluffle.TelegramBot.Routing;
using Fluffle.TelegramBot.Routing.Extensions;
using Nito.AsyncEx;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Fluffle.TelegramBot.Interceptors;

public class ChatRegisterInterceptor : IUpdateInterceptor
{
    private readonly BotContext _context;

    private readonly HashSet<long> _chatIds;
    private readonly AsyncLock _chatIdsLock;

    public ChatRegisterInterceptor(BotContext context)
    {
        _context = context;

        _chatIds = new HashSet<long>();
        _chatIdsLock = new AsyncLock();
    }

    public async Task InterceptAsync(Update update)
    {
        var chat = update.EffectiveChat();
        if (chat == null)
            return;

        using var _ = await _chatIdsLock.LockAsync();
        if (_chatIds.Contains(chat.Id))
            return;

        _chatIds.Add(chat.Id);

        var mongoChat = await _context.Chats.FirstOrDefaultAsync(x => x.Id == chat.Id);
        if (mongoChat != null)
            return;

        await _context.Chats.UpsertAsync(chat, null, chat.Type == ChatType.Private ? chat.Id : null, null);
    }
}

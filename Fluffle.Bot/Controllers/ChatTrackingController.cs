using Noppes.Fluffle.Bot.Database;
using Noppes.Fluffle.Bot.Routing;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Noppes.Fluffle.Bot.Controllers
{
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
            await RateLimiter.RunAsync(message.Chat, () => _botClient.SendTextMessageAsync(message.From.Id, Template.Start(), ParseMode.MarkdownV2));
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
}

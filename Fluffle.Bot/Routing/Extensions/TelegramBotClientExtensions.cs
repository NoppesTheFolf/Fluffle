using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Noppes.Fluffle.Bot.Routing;

public static class TelegramBotClientExtensions
{
    public static async Task<User> GetChatOwnerAsync(this ITelegramBotClient botClient, ChatId chatId)
    {
        var administrators = await botClient.GetChatAdministratorsAsync(chatId);

        return administrators.First(x => x.Status == ChatMemberStatus.Creator).User;
    }
}

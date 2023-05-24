using Telegram.Bot.Types;

namespace Noppes.Fluffle.Bot.Routing;

public static class UpdateExtensions
{
    public static Message EffectiveMessage(this Update update)
    {
        if (update.Message != null)
            return update.Message;

        if (update.EditedMessage != null)
            return update.EditedMessage;

        if (update.CallbackQuery != null)
            return update.CallbackQuery.Message;

        if (update.ChannelPost != null)
            return update.ChannelPost;

        if (update.EditedChannelPost != null)
            return update.EditedChannelPost;

        return null;
    }

    public static Chat EffectiveChat(this Update update)
    {
        if (update.Message != null)
            return update.Message.Chat;

        if (update.EditedMessage != null)
            return update.EditedMessage.Chat;

        if (update.CallbackQuery is { Message: { } })
            return update.CallbackQuery.Message.Chat;

        if (update.ChannelPost != null)
            return update.ChannelPost.Chat;

        if (update.EditedChannelPost != null)
            return update.EditedChannelPost.Chat;

        if (update.MyChatMember != null)
            return update.MyChatMember.Chat;

        if (update.ChatMember != null)
            return update.ChatMember.Chat;

        if (update.ChatJoinRequest != null)
            return update.ChatJoinRequest.Chat;

        return null;
    }
}

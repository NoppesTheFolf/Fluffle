using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Noppes.Fluffle.Bot.Database
{
    public static class BotChatExtensions
    {
        private static readonly Dictionary<ChatType, ReverseSearchFormat> ReverseSearchFormatDefaults = new()
        {
            { ChatType.Private, ReverseSearchFormat.InlineKeyboard },
            { ChatType.Group, ReverseSearchFormat.Text },
            { ChatType.Supergroup, ReverseSearchFormat.Text },
            { ChatType.Channel, ReverseSearchFormat.Text }
        };

        private const InlineKeyboardFormat InlineKeyboardFormatDefault = InlineKeyboardFormat.Multiple;

        private const TextFormat TextFormatDefault = TextFormat.PlatformNames;

        private const string TextSeparatorDefault = "|";

        public static async Task UpsertAsync(this IRepository<MongoChat> repository, Chat tgChat, bool? isActive, long? ownerId, ChatMember botChatMember)
        {
            var chat = await repository.FirstOrDefaultAsync(x => x.Id == tgChat.Id);

            var title = tgChat.Type == ChatType.Private ? tgChat.Username : tgChat.Title;
            chat ??= new MongoChat
            {
                Id = tgChat.Id,
                Type = tgChat.Type,
                OwnerId = ownerId,
                ReverseSearchFormat = ReverseSearchFormatDefaults[tgChat.Type],
                InlineKeyboardFormat = InlineKeyboardFormatDefault,
                TextFormat = TextFormatDefault,
                TextSeparator = TextSeparatorDefault
            };

            chat.Title = title;
            chat.LinkedChatId = tgChat.LinkedChatId;

            if (isActive != null)
                chat.IsActive = isActive;

            if (ownerId != null)
                chat.OwnerId = ownerId;

            if (botChatMember is ChatMemberAdministrator administrator)
            {
                chat.IsAnonymous = administrator.IsAnonymous;
                chat.CanManageChat = administrator.CanManageChat;
                chat.CanDeleteMessages = administrator.CanDeleteMessages;
                chat.CanManageVoiceChats = administrator.CanManageVoiceChats;
                chat.CanRestrictMembers = administrator.CanRestrictMembers;
                chat.CanPromoteMembers = administrator.CanPromoteMembers;
                chat.CanChangeInfo = administrator.CanChangeInfo;
                chat.CanInviteUsers = administrator.CanInviteUsers;
                chat.CanPostMessages = administrator.CanPostMessages;
                chat.CanEditMessages = administrator.CanEditMessages;
                chat.CanPinMessages = administrator.CanPinMessages;
            }
            else
            {
                chat.IsAnonymous = null;
                chat.CanManageChat = null;
                chat.CanDeleteMessages = null;
                chat.CanManageVoiceChats = null;
                chat.CanRestrictMembers = null;
                chat.CanPromoteMembers = null;
                chat.CanChangeInfo = null;
                chat.CanInviteUsers = null;
                chat.CanPostMessages = null;
                chat.CanEditMessages = null;
                chat.CanPinMessages = null;
            }

            await repository.UpsertAsync(x => x.Id == tgChat.Id, chat);
        }

        public static async Task<IList<(string title, long id)>> GetOwnedChatsAsync(this IRepository<MongoChat> repository, long ownerId)
        {
            var chats = await repository.ManyAsync(x => x.OwnerId == ownerId);

            return chats.Select(x =>
            {
                var isOwner = x.Id == ownerId;
                return (isOwner, title: isOwner ? "This chat" : x.Title, id: x.Id);
            }).OrderByDescending(x => x.isOwner).ThenBy(x => x.title).Select(x => (x.title, chatId: x.id)).ToList();
        }
    }
}

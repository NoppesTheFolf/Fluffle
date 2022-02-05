using Telegram.Bot.Types.Enums;

namespace Noppes.Fluffle.Bot.Database
{
    public class MongoChat
    {
        // Data from Telegram

        public long Id { get; set; }

        public string Title { get; set; }

        public ChatType Type { get; set; }

        public bool? IsActive { get; set; }

        public long? OwnerId { get; set; }

        public long? LinkedChatId { get; set; }

        // Settings

        public ReverseSearchFormat ReverseSearchFormat { get; set; }

        public TextFormat TextFormat { get; set; }

        public string TextSeparator { get; set; }

        public InlineKeyboardFormat InlineKeyboardFormat { get; set; }

        // Permissions

        public bool? IsAnonymous { get; set; }

        public bool? CanManageChat { get; set; }

        public bool? CanDeleteMessages { get; set; }

        public bool? CanManageVoiceChats { get; set; }

        public bool? CanRestrictMembers { get; set; }

        public bool? CanPromoteMembers { get; set; }

        public bool? CanChangeInfo { get; set; }

        public bool? CanInviteUsers { get; set; }

        public bool? CanPostMessages { get; set; }

        public bool? CanEditMessages { get; set; }

        public bool? CanPinMessages { get; set; }
    }
}

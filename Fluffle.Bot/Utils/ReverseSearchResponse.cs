using Noppes.Fluffle.Bot.Routing;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Noppes.Fluffle.Bot.Utils
{
    public class ReverseSearchResponse
    {
        public string ExistingText { get; set; }

        public MessageEntity[] ExistingTextEntities { get; set; }

        public Chat Chat { get; set; }

        public string FileId { get; set; }

        public int? MessageId { get; set; }

        public int? ReplyToMessageId { get; set; }

        public bool IsTextCaption { get; set; }

        public string Text { get; set; }

        public MessageEntity[] TextEntities { get; set; }

        public PhotoSize Photo { get; set; }

        public InlineKeyboardMarkup ReplyMarkup { get; set; }

        public async Task<Message> Process(ITelegramBotClient botClient)
        {
            if (MessageId != null)
            {
                if (Text == null)
                    return await RateLimiter.RunAsync(Chat, () => botClient.EditMessageReplyMarkupAsync(Chat.Id, (int)MessageId, ReplyMarkup));

                if (IsTextCaption)
                    return await RateLimiter.RunAsync(Chat, () => botClient.EditMessageCaptionAsync(Chat.Id, (int)MessageId, Text, null, TextEntities));

                return await RateLimiter.RunAsync(Chat, () => botClient.EditMessageTextAsync(Chat.Id, (int)MessageId, Text, null, TextEntities, true));
            }

            if (FileId != null)
                return await RateLimiter.RunAsync(Chat, () => botClient.SendPhotoAsync(Chat.Id, new InputMedia(FileId), Text, null, TextEntities, true, ReplyToMessageId, null, ReplyMarkup));

            return await RateLimiter.RunAsync(Chat, () => botClient.SendTextMessageAsync(Chat.Id, Text, null, TextEntities, true, true, ReplyToMessageId, null, ReplyMarkup));
        }
    }
}

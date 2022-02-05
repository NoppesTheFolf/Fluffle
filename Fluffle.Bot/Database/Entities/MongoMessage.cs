using MongoDB.Bson;
using System;

namespace Noppes.Fluffle.Bot.Database
{
    public class MongoMessage
    {
        public ObjectId Id { get; set; }

        public long ChatId { get; set; }

        public int MessageId { get; set; }

        /// <summary>
        /// The globally unique ID of the photo that got reverse searched.
        /// </summary>
        public string FileUniqueId { get; set; }

        /// <summary>
        /// Response retrieved from Fluffle when the image got reverse searched.
        /// </summary>
        public FluffleResponse FluffleResponse { get; set; }

        public int? ResponseMessageId { get; set; }

        public string Caption { get; set; }

        public Telegram.Bot.Types.MessageEntity[] CaptionEntities { get; set; }

        /// <summary>
        /// ID of the media group the message belongs to.
        /// </summary>
        public string MediaGroupId { get; set; }

        /// <summary>
        /// Time at which the message got processed.
        /// </summary>
        public DateTime When { get; set; }

        // Chat settings used when the message was received

        public ReverseSearchFormat ReverseSearchFormat { get; set; }

        public TextFormat TextFormat { get; set; }

        public string TextSeparator { get; set; }

        public InlineKeyboardFormat InlineKeyboardFormat { get; set; }
    }
}

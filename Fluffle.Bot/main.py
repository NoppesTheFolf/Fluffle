from telegram.message import Message
from telegram.utils.helpers import escape_markdown
from routes.basic_commands import help_command, i_has_found_bug, start_command
from routes.track_chats import handle_mention, track_chat_membership
from settings_menu import register as register_settings_menu
from config import load as load_config
from telegram import Update
from telegram.ext import Updater, Filters, CallbackContext, MessageHandler, ChatMemberHandler, CommandHandler
import telegram.constants as tgc
from reverse_search import reverse_search, ReverseSearchResponse, Formatter
from mongo import MongoMessage, ReverseSearchFormat, database
from threading import Lock
from datetime import datetime
import logging


# Load config
config = load_config()
lock = Lock()


def handle_photo(update: Update, context: CallbackContext):
    # No idea what this type of chat is supposed to be, so we skip it
    if update.effective_chat.type == tgc.CHAT_SENDER:
        return
    
    # Get the chat from the database
    chat = database.chat.find_one_by_id(update.effective_chat.id)

    # Skip messages in supergroups of which the message in forwarded from their linked channel
    if update.effective_chat.type == tgc.CHAT_SUPERGROUP and update.effective_message.forward_from_chat and update.effective_message.forward_from_chat.id == chat.linked_chat_id:
        return
    
    # Skip forwarded messages in channels, those cannot be edited and creating a channel post would be an awful way of solving this issue. The only real way is to link a discussion group
    if update.effective_chat.type == tgc.CHAT_CHANNEL and update.effective_message.forward_from_chat:
        return
    
    # Skip messages that already have at least one source attached to them. Only applies to channels, groups and supergroups
    if update.effective_chat.type != tgc.CHAT_PRIVATE:
        if update.effective_message.caption:
            caption = update.effective_message.caption.casefold()
            if any(url in caption for url in config.telegram_known_sources):
                return
        
        if update.effective_message.reply_markup:
            options = [item for sublist in update.effective_message.reply_markup.inline_keyboard for item in sublist]
            options = list(map(lambda x: x.url.casefold(), filter(lambda x: x.url is not None, options)))
            for option in options:
                if any(url in option for url in config.telegram_known_sources):
                    return
    
    photo, results = reverse_search(context.bot, update.effective_message.photo)

    def process_edit(tg_message: Message):
        message = database.message.find_one({ 'chat_id': tg_message.chat_id, 'message_id': tg_message.message_id })
        message.caption_has_been_edited = message.caption != tg_message.caption
        message.results = list(map(lambda x: x.__dict__, results))
        database.message.upsert_one(message)

        return

    if update.edited_message:
        process_edit(update.edited_message)
        return
    
    if update.edited_channel_post:
        process_edit(update.edited_channel_post)
        return
    
    with lock:
        first_in_media_group = None
        if update.effective_message.media_group_id:
            first_in_media_group = not database.message.any({ 'media_group_id': update.effective_message.media_group_id })
        
        message = MongoMessage(
            _id = None,
            chat_id = update.effective_chat.id,
            reverse_search_format = chat.reverse_search_format,
            text_format = chat.text_format,
            message_id = update.effective_message.message_id,
            caption = update.effective_message.caption,
            caption_has_been_edited = False,
            media_group_id = update.effective_message.media_group_id,
            processed_message_id = None,
            results = list(map(lambda x: x.__dict__, results)),
            when = datetime.utcnow()
        )
        database.message.insert_one(message)
    
    try:
        if update.effective_chat.type == tgc.CHAT_PRIVATE:
            response = ReverseSearchResponse(photo, results, update.effective_message.chat_id, None, None, None, None, None, None)
            response.file_id = photo.file_id
            context.bot.delete_message(update.effective_message.chat_id, update.effective_message.message_id)

            if (len(results) == 0):
                context.bot.send_photo(
                    update.effective_message.chat_id,
                    photo.file_id,
                    caption='This image could not be found.'
                )
            else:
                Formatter.route(chat, response)
                message.processed_message_id = response.process(context.bot).message_id
            
            return

        if update.effective_chat.type in [tgc.CHAT_GROUP, tgc.CHAT_SUPERGROUP]:
            if update.effective_message.media_group_id is None:
                if len(results) == 0:
                    return
                
                response = ReverseSearchResponse(photo, results, update.effective_chat.id, update.effective_message.message_id, None, None, None, None, None)
                Formatter.route(chat, response)
                if response.text is None:
                    response.text = escape_markdown('🦊🔍...', version=2)
                
                message.processed_message_id = response.process(context.bot).message_id
            
            return
        
        if update.effective_message.from_user and update.effective_message.from_user.is_bot and update.effective_message.from_user.id != tgc.ANONYMOUS_ADMIN_ID:
            return
        
        if update.effective_chat.type == tgc.CHAT_CHANNEL:
            response = ReverseSearchResponse(photo, results, update.effective_chat.id, None, None, None, None, update.effective_message.message_id, update.effective_message.caption)
            message.processed_message_id = update.effective_message.message_id

            if len(results) > 0:
                # Use the text format if the message is part of a media group
                chat.reverse_search_format = ReverseSearchFormat.TEXT if update.effective_message.media_group_id else chat.reverse_search_format
                Formatter.route(chat, response)
                response.process(context.bot)

            return
    
    finally:
        database.message.upsert_one(message)


def main() -> None:
    # Enable logging
    logging.basicConfig(
        format='%(asctime)s - %(name)s - %(levelname)s - %(message)s', level=logging.INFO
    )
    
    # Configure Telegram bot
    updater = Updater(config.telegram_token, workers=config.telegram_workers)
    dispatcher = updater.dispatcher

    # Register the start and help commands
    updater.dispatcher.add_handler(CommandHandler('start', start_command, run_async=True))
    updater.dispatcher.add_handler(CommandHandler('help', help_command, run_async=True))
    updater.dispatcher.add_handler(CommandHandler('ihasfoundbug', i_has_found_bug, run_async=True))

    # Register the settings menu
    register_settings_menu(updater)

    # Handle mentions in groups and channels
    dispatcher.add_handler(MessageHandler(Filters.text, handle_mention))

    # Handle incoming images
    dispatcher.add_handler(MessageHandler(Filters.photo, handle_photo, run_async=True))

    # Keep track of which chats the bot is in
    dispatcher.add_handler(ChatMemberHandler(track_chat_membership, ChatMemberHandler.MY_CHAT_MEMBER))

    # Start listening
    updater.start_polling()
    updater.idle()


if __name__ == '__main__':
    main()

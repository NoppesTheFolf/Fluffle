from telegram.ext import CallbackContext, CommandHandler, ConversationHandler, MessageHandler, Filters
from telegram.ext.callbackqueryhandler import CallbackQueryHandler
from telegram.ext.updater import Updater
from telegram.inline.inlinekeyboardbutton import InlineKeyboardButton
from telegram.inline.inlinekeyboardmarkup import InlineKeyboardMarkup
from telegram.parsemode import ParseMode
from telegram.update import Update
from telegram.utils.helpers import escape_markdown
import telegram.constants as tgc
import mongo
from mongo import TextFormat, ReverseSearchFormat, TextSeparator, database
import itertools
import json
from utils import get_owner
import rate_limiter
import grapheme


SELECT_FORMAT, SET_FORMAT = range(2)
SELECT_TEXT_FORMAT, SET_TEXT_FORMAT = range(2)
SELECT_SEPARATOR, SET_SEPARATOR = range(2)


def select_chat(update: Update, context: CallbackContext, text: str, state: int, nextFunc):
    chats = mongo.get_chat({ 'owner_id': update.effective_user.id, 'is_active': True })
    if len(chats) == 1 and chats[0]._id == update.effective_user.id:
        chat = chats[0]

        return nextFunc(update, context, json.dumps(chat._id))

    chats = list(map(lambda x: [
        InlineKeyboardButton(
            text = 'This chat' if x._id == update.effective_user.id else x.title,
            callback_data = json.dumps(x._id)
        )
    ], chats))

    rate_limiter.run(
        context.bot.send_message,
        chat_id = update.effective_chat.id,
        text = text,
        reply_markup = InlineKeyboardMarkup(chats)
    )
    
    return state


def select_option(update: Update, context: CallbackContext, text: str, options, create_option, callback_query_data = None):
    data = json.loads(callback_query_data if callback_query_data else update.callback_query.data)
    
    if update.callback_query:
        available_options = list(itertools.chain(*update.callback_query.message.reply_markup.inline_keyboard))
        chosen_option = next(filter(lambda x: x.callback_data == update.callback_query.data, available_options))
        rate_limiter.run(
            context.bot.edit_message_reply_markup,
            chat_id = update.callback_query.message.chat_id,
            message_id = update.callback_query.message.message_id,
            reply_markup = InlineKeyboardMarkup([[chosen_option]])
        )
        update.callback_query.answer()

    if options is None:
        return data

    rate_limiter.run(
        context.bot.send_message,
        chat_id = update.effective_chat.id,
        text = escape_markdown(text, version = 2),
        reply_markup = InlineKeyboardMarkup(list(map(lambda x: [InlineKeyboardButton(x[0], callback_data = json.dumps(create_option(data, x[1])))], options))),
        parse_mode = ParseMode.MARKDOWN_V2
    )

    return data


def select_format(update: Update, context: CallbackContext, callback_query_data = None):
    select_option(
        update,
        context,
        'Which reverse search format would you like to use? Check out https://fluffle.xyz/bot/#response-format for more information.',
        [
            ('Inline keyboard', ReverseSearchFormat.INLINE_KEYBOARD),
            ('Text', ReverseSearchFormat.TEXT)
        ],
        lambda data, option: { 'chat_id': data, 'format': option },
        callback_query_data
    )

    return SET_FORMAT


def select_text_format(update: Update, context: CallbackContext, callback_query_data = None):
    select_option(
        update,
        context,
        'Which text format would you like to use? Check out https://fluffle.xyz/bot/#response-format for more information.',
        [
            ('Platform names', TextFormat.PLATFORM_NAMES),
            ('Compact links', TextFormat.COMPACT),
            ('Expanded links', TextFormat.EXPANDED)
        ],
        lambda data, option: { 'chat_id': data, 'format': option },
        callback_query_data
    )

    return SET_TEXT_FORMAT


def set_option(data: dict, update: Update, context: CallbackContext, constants_class, select_key, update_settings, text):
    if constants_class is None:
        option = select_key(data)
    else:
        options = [value for name, value in vars(constants_class).items() if not name.startswith('_')]
        option = next(filter(lambda x: x == select_key(data), options))

    chat = mongo.get_chat(data['chat_id'])
    if chat.type == tgc.CHAT_PRIVATE:
        if chat.owner_id != update.effective_user.id:
            return
    else:
        owner = get_owner(context, chat._id)

        if owner.id != update.effective_user.id:
            chat.owner_id = owner.id
            database.chat.upsert_one(chat)

            rate_limiter.run(
                context.bot.send_message,
                chat_id = update.effective_chat.id,
                text = escape_markdown('You are not the owner of the selected chat anymore. Therefore, you are not allowed to edit its settings.', version = 2),
                parse_mode = ParseMode.MARKDOWN_V2
            )
            return

    update_settings(chat, option)
    database.chat.upsert_one(chat)

    rate_limiter.run(
        context.bot.send_message,
        chat_id = update.effective_chat.id,
        text = escape_markdown(text, version = 2),
        parse_mode = ParseMode.MARKDOWN_V2
    )

    return ConversationHandler.END


def set_format(update: Update, context: CallbackContext):
    def set_reverse_search_format(chat, option):
        chat.reverse_search_format = option

    data = select_option(update, context, None, None, None)
    return set_option(
        data,
        update,
        context,
        ReverseSearchFormat,
        lambda x: x['format'],
        set_reverse_search_format,
        'Reverse search format set.'
    )


def set_text_format(update: Update, context: CallbackContext):
    def set_message_search_format(chat, option):
        chat.text_format = option

    data = select_option(update, context, None, None, None)
    return set_option(
        data,
        update,
        context,
        TextFormat,
        lambda x: x['format'],
        set_message_search_format,
        'Text format set.'
    )


def select_separator(update: Update, context: CallbackContext, callback_query_data = None):
    select_option(
        update,
        context,
        'Which separator would you like to use?',
        [
            (f'Vertical bar {TextSeparator.VERTICAL_BAR}', TextSeparator.VERTICAL_BAR),
            (f'Forward slash {TextSeparator.FORWARD_SLASH}', TextSeparator.FORWARD_SLASH),
            (f'Bullet {TextSeparator.BULLET}', TextSeparator.BULLET),
            ('A custom one', TextSeparator.CUSTOM)
        ],
        lambda data, option: { 'chat_id': data, 'separator': option },
        callback_query_data
    )

    return SET_SEPARATOR

def set_separator(update: Update, context: CallbackContext):
    def set_chat_text_separator(chat, option):
        chat.text_separator = option

    def set_chat_text_separator_option(data, constants_class):
        separator = data['separator']
        return set_option(
            data, update, context, constants_class, lambda x: x['separator'],
            set_chat_text_separator, f'Custom separator set to {separator}.'
        )

    if update.callback_query:
        data = select_option(update, context, None, None, None, None)
        if data['separator'] == TextSeparator.CUSTOM:
            rate_limiter.run(
                context.bot.send_message,
                chat_id = update.effective_chat.id,
                text = escape_markdown('Which character would you like to use a custom separator? Please type it in the chat. This may be anything including emojis and other unicode symbols.', version = 2),
                parse_mode = ParseMode.MARKDOWN_V2
            )
            context.user_data['chat_id'] = data['chat_id']

            return SET_SEPARATOR

        return set_chat_text_separator_option(data, TextSeparator)

    if grapheme.length(update.effective_message.text) > 1:
        rate_limiter.run(
            context.bot.send_message,
            chat_id = update.effective_chat.id,
            text = escape_markdown('A separator may only consist out of one character. Please try again.', version = 2),
            parse_mode = ParseMode.MARKDOWN_V2
        )
        return SET_SEPARATOR

    return set_chat_text_separator_option({
        'chat_id': context.user_data['chat_id'],
        'separator': update.effective_message.text
    }, None)


def cancel(update: Update, context: CallbackContext):
    rate_limiter.run(
        context.bot.send_message,
        chat_id = update.effective_chat.id,
        text = 'Action has been cancelled.'
    )

    return ConversationHandler.END


def register(updater: Updater):
    def generate_conversation_handler(command: str, func, states: dict):
        return ConversationHandler(
            entry_points=[CommandHandler(command, func)],
            states=states,
            fallbacks=[CommandHandler('cancel', cancel)]
        )
    
    updater.dispatcher.add_handler(generate_conversation_handler(
        command = 'setformat',
        func = lambda update, context: select_chat(update, context, 'Of which chat would you like to set the reverse search format?', SELECT_FORMAT, select_format),
        states = {
            SELECT_FORMAT: [CallbackQueryHandler(select_format)],
            SET_FORMAT: [CallbackQueryHandler(set_format)],
        }
    ))

    updater.dispatcher.add_handler(generate_conversation_handler(
        command = 'settextformat',
        func = lambda update, context: select_chat(update, context, 'Of which chat would you like to set the text format?', SELECT_TEXT_FORMAT, select_text_format),
        states = {
            SELECT_TEXT_FORMAT: [CallbackQueryHandler(select_text_format)],
            SET_TEXT_FORMAT: [CallbackQueryHandler(set_text_format)],
        }
    ))

    updater.dispatcher.add_handler(generate_conversation_handler(
        command = 'settextseparator',
        func = lambda update, context: select_chat(update, context, 'Of which chat would you like to change the text separator?', SELECT_SEPARATOR, select_separator),
        states = {
            SELECT_SEPARATOR: [CallbackQueryHandler(select_separator)],
            SET_SEPARATOR: [MessageHandler(Filters.text & ~Filters.command, set_separator), CallbackQueryHandler(set_separator)],
        }
    ))

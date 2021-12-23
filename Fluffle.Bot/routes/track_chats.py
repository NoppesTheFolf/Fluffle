from mongo import upsert_chat
from telegram import Update
from telegram import Chat
from telegram.chatmember import ChatMember
import telegram.constants as tgc
from telegram.ext import CallbackContext
from utils import get_owner


def update_chat(context: CallbackContext, chat: Chat, is_active: bool, botChatMember: ChatMember = None):
    owner = None
    if is_active:
        owner = get_owner(context, chat.id)
    
    upsert_chat(chat, is_active, owner, botChatMember)


def track_chat_membership(update: Update, context: CallbackContext) -> None:
    is_active = update.my_chat_member.new_chat_member.status == tgc.CHATMEMBER_ADMINISTRATOR
    chat = context.bot.get_chat(update.effective_chat.id) if is_active else update.effective_chat
    
    update_chat(context, chat, is_active, update.my_chat_member.new_chat_member)


def remove_mention(context: CallbackContext):
    context.bot.delete_message(
        chat_id = context.job.context[0],
        message_id = context.job.context[1]
    )


def handle_mention(update: Update, context: CallbackContext):
    if not update.effective_message.text.startswith('@'):
        return
    
    if update.effective_chat.type not in [tgc.CHAT_GROUP, tgc.CHAT_SUPERGROUP, tgc.CHAT_CHANNEL]:
        return
    
    received_username = update.effective_message.text[1:].casefold()
    bot_username = context.bot.username.casefold()
    if received_username != bot_username:
        return
    
    chat = context.bot.get_chat(update.effective_chat.id)
    update_chat(context, chat, True)
    context.job_queue.run_once(remove_mention, 1, context=(update.effective_chat.id, update.effective_message.message_id))

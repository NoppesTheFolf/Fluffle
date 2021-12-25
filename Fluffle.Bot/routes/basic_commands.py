from mongo import upsert_chat
from template import Template, send_template
from telegram import Update
import telegram.constants as tgc
from telegram.ext import CallbackContext


def start_command(update: Update, context: CallbackContext) -> None:
    if update._effective_chat.type != tgc.CHAT_PRIVATE:
        return
    
    upsert_chat(update.effective_chat, True, update.effective_user)
    send_template(update.message, Template.START)


def help_command(update: Update, context: CallbackContext):
    if update._effective_chat.type != tgc.CHAT_PRIVATE:
        return
    
    send_template(update.message, Template.HELP)


def i_has_found_bug(update: Update, context: CallbackContext):
    if update._effective_chat.type != tgc.CHAT_PRIVATE:
        return
    
    send_template(update.message, Template.I_HAS_FOUND_BUG)
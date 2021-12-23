from telegram.ext import CallbackContext
import telegram.constants as tgc
from telegram.user import User


def get_owner(context: CallbackContext, chatId: int) -> User:
    administrators = context.bot.get_chat_administrators(chatId)
    owner = next(filter(lambda x: x.status == tgc.CHATMEMBER_CREATOR, administrators))

    return owner.user

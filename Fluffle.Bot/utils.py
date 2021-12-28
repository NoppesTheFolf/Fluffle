from telegram.ext import CallbackContext
import telegram.constants as tgc
from telegram.user import User
import rate_limiter


def get_owner(context: CallbackContext, chat_id: int) -> User:
    administrators = rate_limiter.run(
        context.bot.get_chat_administrators,
        chat_id = chat_id
    )
    owner = next(filter(lambda x: x.status == tgc.CHATMEMBER_CREATOR, administrators))

    return owner.user

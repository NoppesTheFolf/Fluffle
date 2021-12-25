from telegram.message import Message
from telegram.parsemode import ParseMode
from enum import Enum
import chevron


class Template(Enum):
    START = 1
    HELP = 2
    I_HAS_FOUND_BUG = 3


cache = {}
options = [(name, value) for name, value in vars(Template).items() if not name.startswith('_')]
for name, value in [(name, value) for name, value in vars(Template).items() if not name.startswith('_')]:
    name = name.lower()
    with open(f'templates/{name}.mustache', 'r') as file:
        template = file.read()

    cache[value] = template.replace('.', '\\.')


def send_template(message: Message, template: Template, data: dict = {}) -> None:
    message.reply_text(chevron.render(
        template = cache[template],
        data = data
    ), parse_mode=ParseMode.MARKDOWN_V2)

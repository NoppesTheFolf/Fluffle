from config import load as load_config
from telegram import Update, ParseMode
import logging
from telegram.ext import Updater, Filters, CallbackContext, MessageHandler
import telegram.constants as tgc
import tempfile
import fluffle
from itertools import groupby
import os


def handle_document(update: Update, context: CallbackContext):
    context.bot.send_message(
        chat_id=update.effective_chat.id,
        text='I don\'t support documents at the moment.'
    )


def handle_photo(update: Update, context: CallbackContext):
    if update.effective_chat.type != tgc.CHAT_PRIVATE:
        return

    handle_photo_private(update, context)


def handle_photo_private(update: Update, context: CallbackContext):
    # Select the best image to reverse search from the ones Telegram provides
    photos = update.message.photo
    photos = map(lambda p: (p, min(p.width, p.height)), photos)
    photos = map(lambda p: (p[0], float('inf') if p[1] < 512 else p[1]), photos)
    photos = sorted(photos, key=lambda p: p[1])
    photo = photos[0][0]

    # Download the selected image from the Telegram servers
    with tempfile.NamedTemporaryFile(delete=False) as tmp:
        name = tmp.name
    
    try:
        context.bot.get_file(photo).download(name)
        results = fluffle.search(name)
        results = filter(lambda r: r['score'] > 0.92, results)
        results = list(results)

        # Check if there are any reliable results
        if (len(results) == 0):
            context.bot.send_photo(
                update.effective_message.chat_id,
                photo.file_id,
                caption='Couldn\'t find this image, sorry!'
            )
            context.bot.delete_message(update.effective_message.chat_id, update.effective_message.message_id)
            return
        
        # Prepare a nicely formatted response
        response = ''
        for key, group in groupby(sorted(results, key=lambda r: r['platform']), key=lambda r: r['platform']):
            response += '\n*{}*\n'.format(key)
            for item in group:
                response += item['location'] + '\n'
        
        context.bot.send_photo(update.effective_message.chat_id, photo.file_id, caption=response, parse_mode=ParseMode.MARKDOWN)
        context.bot.delete_message(update.effective_message.chat_id, update.effective_message.message_id)
    finally:
        os.remove(name)


def main() -> None:
    # Enable logging
    logging.basicConfig(
        format='%(asctime)s - %(name)s - %(levelname)s - %(message)s', level=logging.INFO
    )

    # Load config
    config = load_config()

    # Configure Telegram bot
    updater = Updater(config.telegram_token, workers=config.telegram_workers)
    dispatcher = updater.dispatcher

    media_handler = MessageHandler(Filters.photo, handle_photo, run_async=True)
    dispatcher.add_handler(media_handler)

    document_handler = MessageHandler(Filters.document, handle_document, run_async=True)
    dispatcher.add_handler(document_handler)

    # Start listening
    updater.start_polling()
    updater.idle()


if __name__ == '__main__':
    main()

import tempfile
from threading import Lock, Semaphore, Thread
from typing import List, Optional, Tuple
from requests import post
from telegram.bot import Bot
from telegram.files.photosize import PhotoSize
from telegram.message import Message
from telegram.utils.helpers import escape_markdown
import config
from dataclasses import dataclass
import os
from itertools import groupby
from telegram import ParseMode, ReplyKeyboardMarkup, InlineKeyboardMarkup, InlineKeyboardButton
from mongo import TextFormat, MongoChat, ReverseSearchFormat
from math import floor
import re
import rate_limiter
from config import get as get_config
from queue import PriorityQueue
from telegram.ext.utils.promise import Promise
from dataclasses import dataclass, field


@dataclass
class ReverseSearchItem:
    id: int
    platform: str
    location: str
    priority: int


WWW_MATCH = regex = r'https?:\/\/www\.'
FUR_AFFINITY = 'furAffinity'
TWITTER = 'twitter'
E621 = 'e621'
WEASYL = 'weasyl'
WEASYL_REGEX = r'\/submission\/([0-9]*)'
FURRY_NETWORK = 'furryNetwork'
PLATFORMS = {
    FUR_AFFINITY: (1, 'Fur Affinity'),
    TWITTER: (2, 'Twitter'),
    E621: (3, 'e621'),
    WEASYL: (4, 'Weasyl'),
    FURRY_NETWORK: (5, 'Furry Network')
}
PLATFORM_SIZE_LOOKUP = {
    'Fur Affinity': 79,
    'Twitter': 54,
    'e621': 36,
    'Weasyl': 51,
    'Furry Network': 100
}

class ReverseSearchResponse:
    def __init__(self, photo: PhotoSize, results: List[ReverseSearchItem], chat_id: int, reply_to_message_id: int, file_id: str, text: str, reply_markup: ReplyKeyboardMarkup, message_id: int, existing_caption: str):
        self.photo = photo
        self.results = results
        self.chat_id = chat_id
        self.reply_to_message_id = reply_to_message_id
        self.file_id = file_id
        self.text = text
        self.reply_markup = reply_markup
        self.message_id = message_id
        self.existing_caption = existing_caption
    
    def process(self, bot: Bot) -> Message:
        # We need to edit either the message or the reply markup if message_id is defined
        if self.message_id:
            if self.text:
                return rate_limiter.run(
                    bot.edit_message_caption,
                    chat_id = self.chat_id,
                    caption = self.text,
                    message_id = self.message_id,
                    reply_markup = self.reply_markup,
                    parse_mode = ParseMode.MARKDOWN_V2
                )
            elif self.reply_markup:
                return rate_limiter.run(
                    bot.edit_message_reply_markup,
                    chat_id = self.chat_id,
                    message_id = self.message_id,
                    reply_markup = self.reply_markup,
                )
        # Else check if we need to send a photo, this is used for private chats
        elif self.file_id:
            return rate_limiter.run(
                bot.send_photo,
                chat_id = self.chat_id,
                reply_to_message_id = self.reply_to_message_id,
                photo = self.file_id,
                caption = self.text,
                reply_markup = self.reply_markup,
                disable_notification = True,
                parse_mode = ParseMode.MARKDOWN_V2
            )
        # Else we send a normal message
        else:
            return rate_limiter.run(
                bot.send_message,
                chat_id = self.chat_id,
                reply_to_message_id = self.reply_to_message_id,
                text = self.text,
                reply_markup = self.reply_markup,
                parse_mode = ParseMode.MARKDOWN_V2,
                disable_notification = True,
                disable_web_page_preview = True
            )


class Formatter:
    def route(chat: MongoChat, response: ReverseSearchResponse):
        if chat.reverse_search_format == ReverseSearchFormat.TEXT:
            format = Formatter.use_message
        elif chat.reverse_search_format == ReverseSearchFormat.INLINE_KEYBOARD:
            format = Formatter.use_inline_keyboard
        else:
            return

        format(chat, response)

    def use_message(chat: MongoChat, response: ReverseSearchResponse):
        if chat.text_format == TextFormat.COMPACT:
            format = lambda results: Formatter.format_message(results, False)
        elif chat.text_format == TextFormat.EXPANDED:
            format = lambda results: Formatter.format_message(results, True)
        elif chat.text_format == TextFormat.PLATFORM_NAMES:
            format = lambda results: Formatter.format_platform_names(results, chat.text_separator)
        else:
            return

        text, caption_after = format(list(sorted(response.results, key=lambda r: r.priority)))
        if response.existing_caption is not None:
            prefix = '' if caption_after else text + '\n\n'
            suffix = '\n\n' + text if caption_after else ''
            text = prefix + escape_markdown(response.existing_caption, 2) + suffix

        response.text = text

    def format_platform_names(results: List[ReverseSearchItem], separator):
        separator = escape_markdown(separator, 2)
        text = f' {separator} '.join(map(lambda result: f'[{result.platform}]({result.location})', results))
        
        return text, False

    TEXT_FORMAT_LIMIT = 3
    
    def format_message(results: List[ReverseSearchItem], expanded: bool) -> str:
        text = ''
        for key, group in groupby(results[:Formatter.TEXT_FORMAT_LIMIT], key=lambda r: r.platform):
            if expanded:
                text += '\n*{}*\n'.format(key)
            
            for item in group:
                text += escape_markdown(item.location, 2) + '\n'
        
        text = text.strip()

        return text, True

    def use_inline_keyboard(chat: MongoChat, response: ReverseSearchResponse): 
        aspect_ratio = response.photo.width / response.photo.height
        # At an aspect ratio of 0.25 everything in the keyboard basically becomes unreadable anyway
        aspect_ratio = 0.25 if aspect_ratio < 0.25 else aspect_ratio
        # Any aspect ratio larger than 1.0 will not grow bigger anymore
        aspect_ratio = 1.0 if aspect_ratio > 1.0 else aspect_ratio

        bin_options = [
            (1, floor(275 * aspect_ratio)),
            (2, floor(134 * aspect_ratio)),
            (3, floor(90 * aspect_ratio))
        ]

        def compute_bins(results):
            # Sort platform names, from longest to shortest
            results.sort(key=lambda x: PLATFORM_SIZE_LOOKUP[x.platform], reverse=True)

            bins = []
            index = 0
            while True:
                item = results[index]
                # Oh no what have I done, but it works, do not touch it :P
                bin = next(map(lambda x: x[0], iter(sorted(filter(lambda x: x[1] > 1, map(lambda bin: [bin, bin[1]/ PLATFORM_SIZE_LOOKUP[item.platform]], bin_options)), key = lambda x: x[1]))), bin_options[0])

                new_index = index + bin[0]
                bins.append(results[index:new_index])
                if new_index >= len(results):
                    break

                index = new_index

            return bins

        results = response.results.copy()
        while True:
            bins = compute_bins(results)

            if len(bins) < 3:
                break

            results.remove(max(results, key = lambda x: x.priority))

        # Create the inline keyboard markup
        buttons = list(map(lambda x: list(map(lambda y: InlineKeyboardButton(y.platform, y.location), x)), bins))
        response.reply_markup = InlineKeyboardMarkup(buttons)


def _search(name, platforms):
    # Preprocessing will only make the image larger (in terms of file size) and reduce reverse search accuracy

    # Make the request to Fluffle
    headers = {
        'User-Agent': 'telegram-bot/' + config.get_version()
    }
    files = {
        'file': open(name, 'rb')
    }
    data = {
        'limit': 16,
        'includeNsfw': True,
        'platforms': platforms
    }
    response = post(
        'https://api.fluffle.xyz/v1/search',
        headers=headers,
        files=files,
        data=data
    ).json()

    return response['results']


_config = get_config()
THRESHOLD = 512

_chat_locks = dict()
_chat_locks_lock = Lock()


@dataclass(order=True)
class ReverseSearcherRequest:
    priority: int
    promise: Promise = field(compare = False)


class ReverseSearcher:
    _queue: PriorityQueue[ReverseSearcherRequest]
    def __init__(self, workers: int) -> None:
        self._queue = PriorityQueue()
        for _ in range(workers):
            Thread(target = self.target).start()


    def target(self):
        while True:
            request = self._queue.get()
            request.promise()


    def search(self, bot: Bot, chat: MongoChat, photos: List[PhotoSize], priority: int) -> Tuple[PhotoSize, List[ReverseSearchItem]]:
        promise = Promise(_reverse_search, [bot, chat, photos], {})
        self._queue.put(ReverseSearcherRequest(priority, promise))

        return promise.result()


_reverse_searcher = ReverseSearcher(_config.reverse_search_max_concurrency)


def reverse_search(bot: Bot, chat: MongoChat, photos: List[PhotoSize], priority: int) -> Tuple[PhotoSize, List[ReverseSearchItem]]:
    with _chat_locks_lock:
        chat_lock = _chat_locks.get(chat._id)
        if chat_lock is None:
            chat_lock = Semaphore(_config.reverse_search_max_concurrency_per_chat)
            _chat_locks[chat._id] = chat_lock

    with chat_lock:
        return _reverse_searcher.search(bot, chat, photos, priority)


def _reverse_search(bot: Bot, chat: MongoChat, photos: List[PhotoSize]) -> Tuple[PhotoSize, List[ReverseSearchItem]]:
    # Select the best image to reverse search from the ones Telegram provides
    photos = list(map(lambda p: (p, min(p.width, p.height)), photos))
    all_under_threshold = all(x[1] < THRESHOLD for x in photos)
    photos = map(lambda p: (p[0], float('inf') if p[1] < THRESHOLD and not all_under_threshold else p[1]), photos)
    photos = sorted(photos, key=lambda p: p[1], reverse=all_under_threshold)
    photo = photos[0][0]
    
    # Download the selected image from the Telegram servers
    with tempfile.NamedTemporaryFile(delete=False) as tmp:
        name = tmp.name
    
    try:
        bot.get_file(photo).download(name)
        results = _search(name, PLATFORMS.keys())
        results = filter(lambda r: r['match'] == 'exact', results)

        def map_result(item):
            # URLs from Weasyl can be represented in a shorter way
            if item['platform'] == WEASYL:
                match = re.search(WEASYL_REGEX, item['location'])
                if match:
                    item['location'] = f'https://weasyl.com/submission/{match.group(1)}'
            
            # Fur Affinity URLs sometimes have a www. prefix which is useless
            if item['platform'] == FUR_AFFINITY:
                match = re.search(WWW_MATCH, item['location'])
                if match:
                    item['location'] = 'https://' + item['location'][match.span()[1]:]
            
            platform = PLATFORMS[item['platform']][1]
            priority = PLATFORMS[item['platform']][0]
            return ReverseSearchItem(item['id'], platform, item['location'], priority)
        
        return photo, list(map(map_result, results))
    finally:
        os.remove(name)

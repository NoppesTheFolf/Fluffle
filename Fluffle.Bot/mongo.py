from typing import Generic, List, Optional, TypeVar
from pymongo import MongoClient, ASCENDING
from dataclasses import dataclass
from pymongo.collection import Collection
from telegram.chat import Chat
from telegram.chatmember import ChatMember
from telegram.user import User
from config import get as get_config
import copy
import telegram.constants as tgc
from datetime import datetime


@dataclass
class MongoChat:
    _id: int
    title: str
    type: str
    is_active: bool
    # ID of the user that owns the channel and is therefore allowed to make changes to the settings
    owner_id: int
    # Settings regarding presentation of the reverse search results
    reverse_search_format: str
    text_format: str
    # Unique identifier for the linked chat, i.e. the discussion group identifier for a channel and vice versa; for supergroups and channel chats.
    linked_chat_id: Optional[int] = None
    # Permissions
    is_anonymous: Optional[bool] = None
    '''True, if the user's presence in the chat is hidden'''
    can_manage_chat: Optional[bool] = None
    '''True, if the administrator can access the chat event log, chat statistics, message statistics in channels, see channel members, see anonymous administrators in supergroups and ignore slow mode. Implied by any other administrator privilege'''
    can_delete_messages: Optional[bool] = None
    '''True, if the administrator can delete messages of other users'''
    can_manage_voice_chats: Optional[bool] = None
    '''True, if the administrator can manage voice chats'''
    can_restrict_members: Optional[bool] = None
    '''True, if the administrator can restrict, ban or unban chat members'''
    can_promote_members: Optional[bool] = None
    '''True, if the administrator can add new administrators with a subset of their own privileges or demote administrators that he has promoted, directly or indirectly (promoted by administrators that were appointed by the user)'''
    can_change_info: Optional[bool] = None
    '''True, if the user is allowed to change the chat title, photo and other settings'''
    can_invite_users: Optional[bool] = None
    '''True, if the user is allowed to invite new users to the chat'''
    can_post_messages: Optional[bool] = None
    '''True, if the administrator can post in the channel; channels only'''
    can_edit_messages: Optional[bool] = None
    '''Optional. True, if the administrator can edit messages of other users and can pin messages; channels only'''
    can_pin_messages: Optional[bool] = None
    '''True, if the user is allowed to pin messages; groups and supergroups only'''


@dataclass
class MongoMessage:
    _id: int
    chat_id: int
    reverse_search_format: str
    text_format: str
    message_id: int
    caption: str
    caption_has_been_edited: bool
    media_group_id: str
    processed_message_id: int
    when: datetime
    results: Optional[List] = None


class ReverseSearchFormat:
    TEXT = "TEXT"
    INLINE_KEYBOARD = "INLINE_KEYBOARD"


class TextFormat:
    PLATFORM_NAMES = "PLATFORM_NAMES"
    COMPACT = "COMPACT"
    EXPANDED = "EXPANDED"


T = TypeVar('T')
class FluffleCollection(Generic[T]):
    def __init__(self, collection: Collection, create):
        self.__collection = collection
        self.__create = create
    

    def any(self, filter) -> bool:
        return self.__collection.count_documents(filter, limit = 1) == 1


    def find_one(self, filter) -> T:
        dict = self.__collection.find_one(filter)
        
        if dict is None:
            return None
        
        return self.__create(**dict)


    def find_one_by_id(self, id) -> T:
        return self.find_one({ '_id': id })
    
    
    def find(self, filter) -> List[T]:
        results = self.__collection.find(filter)
        
        return list(map(lambda x: self.__create(**x), results))

    
    def insert_one(self, obj):
        obj_dict = copy.deepcopy(obj.__dict__)

        if obj_dict['_id'] is None:
            obj_dict.pop('_id')
        
        result = self.__collection.insert_one(obj_dict)
        obj._id = result.inserted_id


    def upsert_one(self, obj):
        obj_dict = copy.deepcopy(obj.__dict__)

        if obj_dict['_id'] is None:
            id = None
        else:
            id = obj_dict['_id']
            obj_dict.pop('_id')
        
        self.__collection.replace_one({ '_id': id }, obj_dict, upsert=True)


class FluffleDatabase:
    def __init__(self) -> None:
        config = get_config()
        self.__client = MongoClient(config.mongo_uri)
        db = self.__client.bot

        self.chat = FluffleCollection[MongoChat](db.chat, MongoChat)
        db.chat.create_index('owner_id')

        self.message = FluffleCollection[MongoMessage](db.message, MongoMessage)
        db.message.create_index('media_group_id')
        db.message.create_index([('chat_id', ASCENDING),('message_id', ASCENDING)], unique=True)


database = FluffleDatabase()


REVERSE_SEARCH_FORMAT_DEFAULTS = {
    (tgc.CHAT_PRIVATE, True): ReverseSearchFormat.INLINE_KEYBOARD,
    (tgc.CHAT_PRIVATE, False): ReverseSearchFormat.INLINE_KEYBOARD,
    (tgc.CHAT_GROUP, True): ReverseSearchFormat.TEXT,
    (tgc.CHAT_GROUP, False): ReverseSearchFormat.TEXT,
    (tgc.CHAT_SUPERGROUP, True): ReverseSearchFormat.TEXT,
    (tgc.CHAT_SUPERGROUP, False): ReverseSearchFormat.TEXT,
    (tgc.CHAT_CHANNEL, True): ReverseSearchFormat.TEXT,
    (tgc.CHAT_CHANNEL, False): ReverseSearchFormat.INLINE_KEYBOARD
}


TEXT_FORMAT_DEFAULTS = {
    tgc.CHAT_PRIVATE: TextFormat.PLATFORM_NAMES,
    tgc.CHAT_GROUP: TextFormat.PLATFORM_NAMES,
    tgc.CHAT_SUPERGROUP: TextFormat.PLATFORM_NAMES,
    tgc.CHAT_CHANNEL: TextFormat.PLATFORM_NAMES
}


def upsert_chat(tg_chat: Chat, is_active: bool, owner: User = None, botChatMember: ChatMember = None):
    chat = database.chat.find_one_by_id(tg_chat.id)
    
    if chat is None:
        chat = MongoChat(
            _id = tg_chat.id,
            title = owner.username if tg_chat.type == tgc.CHAT_PRIVATE else tg_chat.title,
            type = tg_chat.type,
            linked_chat_id = tg_chat.linked_chat_id,
            is_active = is_active,
            owner_id = owner.id,
            reverse_search_format = REVERSE_SEARCH_FORMAT_DEFAULTS[(tg_chat.type, tg_chat.linked_chat_id is not None)],
            text_format = TEXT_FORMAT_DEFAULTS[tg_chat.type]
        )
    else:
        chat.title = owner.username if tg_chat.type == tgc.CHAT_PRIVATE else tg_chat.title
        chat.linked_chat_id = tg_chat.linked_chat_id
        chat.is_active = is_active
        
        if owner is not None:
            chat.owner_id = owner.id
    
    if botChatMember is not None:
        chat.is_anonymous = botChatMember.is_anonymous
        chat.can_manage_chat = botChatMember.can_manage_chat
        chat.can_delete_messages = botChatMember.can_delete_messages
        chat.can_manage_voice_chats = botChatMember.can_manage_voice_chats
        chat.can_restrict_members = botChatMember.can_restrict_members
        chat.can_promote_members = botChatMember.can_promote_members
        chat.can_change_info = botChatMember.can_change_info
        chat.can_invite_users = botChatMember.can_invite_users
        chat.can_post_messages = botChatMember.can_post_messages
        chat.can_edit_messages = botChatMember.can_edit_messages
        chat.can_pin_messages = botChatMember.can_pin_messages
    
    database.chat.upsert_one(chat)
    return chat

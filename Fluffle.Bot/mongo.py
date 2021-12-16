from typing import Generic, TypeVar
from pymongo import MongoClient
from dataclasses import dataclass
from pymongo.collection import Collection
from config import load as load_config
import copy


@dataclass
class Chat:
    _id: int


@dataclass
class Message:
    _id: int


T = TypeVar('T')
class FluffleCollection(Generic[T]):
    def __init__(self, collection: Collection, create):
        self.__collection = collection
        self.__create = create

    def find_one(self, id) -> T:
        dict = self.__collection.find_one({ '_id': id })
        return self.__create(**dict)

    def upsert_one(self, id, obj):
        obj_dict = copy.deepcopy(obj.__dict__)

        if obj_dict['_id'] is None:
            obj_dict.pop('_id')
        
        self.__collection.replace_one({ '_id': id }, obj_dict, upsert=True)


class FluffleDatabase:
    def __init__(self) -> None:
        config = load_config()
        self.__client = MongoClient(config.mongo_uri)
        db = self.__client.bot

        self.chat = FluffleCollection[Chat](db.chat, Chat)


database = FluffleDatabase()

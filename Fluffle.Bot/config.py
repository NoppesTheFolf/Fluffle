import os
from typing import List
import yaml
from dataclasses import dataclass


@dataclass
class Config:
    telegram_token: str
    telegram_workers: int
    telegram_known_sources: List[str]
    mongo_uri: str


def get_version() -> str:
    return '0.12.7' if os.environ.get('DEV') is None else 'development'


def load() -> Config:
    with open('config.yml', 'r') as config_file:
        config = yaml.load(config_file, Loader=yaml.SafeLoader)

    return Config(**config)

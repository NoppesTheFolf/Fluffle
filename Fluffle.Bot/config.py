import os
from typing import List
import yaml
from dataclasses import dataclass
from copy import deepcopy


@dataclass
class Config:
    telegram_token: str
    telegram_workers: int
    telegram_known_sources: List[str]
    telegram_all_burst_limit: int
    telegram_all_time_limit_ms: int
    telegram_group_burst_limit: int
    telegram_group_time_limit_ms: int
    telegram_read_timeout: int
    telegram_connect_timeout: int
    mongo_uri: str


def get_version() -> str:
    return '0.13.0' if os.environ.get('DEV') is None else 'development'


def _load() -> Config:
    with open('config.yml', 'r') as config_file:
        config = yaml.load(config_file, Loader=yaml.SafeLoader)

    return Config(**config)

config = _load()
def get() -> Config:
    return deepcopy(config)

import os
import yaml


def get_version() -> str:
    return '0.8.2' if os.environ.get('DEV') is None else 'development'


def load() -> dict:
    with open('config.yml', 'r') as config_file:
        config = yaml.load(config_file, Loader=yaml.SafeLoader)

    return config

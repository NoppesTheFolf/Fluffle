from typing import Callable
from threading import Lock
from telegram.ext import DelayQueue
from config import get as get_config
from telegram.ext.utils.promise import Promise


_config = get_config()
_all_delay_queue = DelayQueue(
    burst_limit = _config.telegram_all_burst_limit,
    time_limit_ms = _config.telegram_all_time_limit_ms,
    autostart = True
)

_lock = Lock()
_group_delay_queues = dict()

def run(func: Callable, *args, **kwargs) -> Callable:
    chat_id = kwargs['chat_id']
    if chat_id < 0:
        with _lock:
            group_delay_queue = _group_delay_queues.get(chat_id)
            if group_delay_queue is None:
                group_delay_queue = DelayQueue(
                    burst_limit = _config.telegram_group_burst_limit,
                    time_limit_ms = _config.telegram_group_time_limit_ms,
                    autostart = True
                )
                _group_delay_queues[chat_id] = group_delay_queue
        
        group_promise = Promise(lambda: None, [], {})
        group_delay_queue(group_promise)
        group_promise.result()

    promise = Promise(func, args, kwargs)
    _all_delay_queue(promise)
    return promise.result()

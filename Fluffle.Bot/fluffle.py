from requests import post
from pyvips import Image, Size
import config


def calculate_size(width, height, target):
    def calculate_size(d1, d2, d1_target): return round(d1_target / d1 * d2)

    if width > height:
        return calculate_size(height, width, target), target

    return target, calculate_size(width, height, target)


def search(name):
    # Preprocess the image before sending it
    image = Image.new_from_file(name)
    width, height = calculate_size(image.width, image.height, 256)
    image = image.thumbnail_image(width, height=height, size=Size.FORCE)
    buffer = image.pngsave_buffer()

    # Make the request to Fluffle
    headers = {
        'User-Agent': 'telegram-bot/' + config.get_version()
    }
    files = {
        'file': buffer
    }
    data = {
        'limit': 16,
        'includeNsfw': True
    }
    response = post(
        'https://api.fluffle.xyz/v1/search',
        headers=headers,
        files=files,
        data=data
    ).json()

    return response['results']

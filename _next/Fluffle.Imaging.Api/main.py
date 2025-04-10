from fastapi import FastAPI, UploadFile, Request
from fastapi.responses import JSONResponse
import pyvips
import math
import base64
import hmac
import os

app = FastAPI()

API_KEY = os.environ["API_KEY"]
@app.middleware("http")
async def authenticate(request: Request, call_next):
    api_key = request.headers.get("Api-Key")
    if api_key is None or not hmac.compare_digest(api_key, API_KEY):
        return JSONResponse({
            "detail": "Invalid API key."
        }, status_code=401)

    return await call_next(request)

@app.post("/thumbnail")
async def post_thumbnail(image: UploadFile, size: int, quality: int):
    image_content = await image.read()
    thumbnail, thumbnail_width, thumbnail_height = create_thumbnail(open_image(image_content), size, quality)
    center_x, center_y = calculate_center(open_image(image_content))

    result = {
        "width": thumbnail_width,
        "height": thumbnail_height,
        "centerX": center_x,
        "centerY": center_y,
        "thumbnail": base64.b64encode(thumbnail).decode("utf-8")
    }

    return JSONResponse(result)

def create_thumbnail(img: pyvips.Image, size: int, quality: int):
    if img.interpretation != pyvips.Interpretation.SRGB:
        img = img.colourspace(pyvips.Interpretation.SRGB)

    if img.hasalpha():
        img = img.flatten(background=[255])

    target_width, target_height = img.width, img.height
    scaling_factor = size / min(img.width, img.height)
    if scaling_factor < 1:
        target_width, target_height = round(scaling_factor * img.width), round(scaling_factor * img.height)

    img = img.thumbnail_image(
        target_width,
        height=target_height,
        size=pyvips.Size.FORCE,
        crop=pyvips.Interesting.NONE
    )

    thumbnail = img.jpegsave_buffer(
        Q=quality,
        optimize_coding=True,
        interlace=True,
        subsample_mode=pyvips.ForeignSubsample.ON,
        trellis_quant=True,
        overshoot_deringing=True,
        optimize_scans=True,
        strip=True
    )

    return thumbnail, img.width, img.height

def calculate_center(img: pyvips.Image):
    target = min(img.width, img.height)
    img_cropped = img.smartcrop(target, target, interesting=pyvips.Interesting.ATTENTION)

    xoffset = abs(img_cropped.xoffset)
    yoffset = abs(img_cropped.yoffset)

    xleft = img.width - target
    yleft = img.height - target

    x = math.floor(0 if xleft == 0 else xoffset / xleft * 100)
    y = math.floor(0 if yleft == 0 else yoffset / yleft * 100)

    return x, y

def open_image(image_content: bytes):
    image = pyvips.Image.new_from_buffer(image_content, "", access="sequential")
    image = image.autorot()

    return image

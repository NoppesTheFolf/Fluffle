from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse
import torch
import os
import hmac
from exact_match_v2 import router as exact_match_v2_router
from bluesky_furry_art import router as bluesky_furry_art_router

TORCH_NUM_THREADS = int(os.environ["TORCH_NUM_THREADS"])
if TORCH_NUM_THREADS != -1:
    torch.set_num_threads(TORCH_NUM_THREADS)

API_KEY = os.environ["API_KEY"]

app = FastAPI()

@app.middleware("http")
async def authenticate(request: Request, call_next):
    api_key = request.headers.get("Api-Key")
    if api_key is None or not hmac.compare_digest(api_key, API_KEY):
        return JSONResponse({
            "detail": "Invalid API key."
        }, status_code=401)

    return await call_next(request)

app.include_router(exact_match_v2_router)
app.include_router(bluesky_furry_art_router)

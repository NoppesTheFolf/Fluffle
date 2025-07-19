from fastapi import FastAPI, UploadFile, Request
from fastapi.responses import JSONResponse
import torch
from torchvision import models
from torchvision.transforms import v2
from PIL import Image
import os
import hmac

TORCH_NUM_THREADS = int(os.environ["TORCH_NUM_THREADS"])
if TORCH_NUM_THREADS != -1:
    torch.set_num_threads(TORCH_NUM_THREADS)

API_KEY = os.environ["API_KEY"]

DEVICE = torch.device("cpu")

def load_model(embedding_size, filename):
    backbone = models.convnext_tiny()
    backbone_in_features = backbone.classifier[2].in_features
    backbone.classifier[2] = torch.nn.Identity()
    model = torch.nn.Sequential(
        backbone,
        torch.nn.Dropout(p=0.5),
        torch.nn.Linear(backbone_in_features, embedding_size)
    )
    model.load_state_dict(torch.load(filename, DEVICE, weights_only=True))
    model.eval()

    return model

model_v2 = load_model(64, "exactMatchV2.pt")

backbone_transforms = models.ConvNeXt_Tiny_Weights.IMAGENET1K_V1.transforms()
transforms = v2.Compose([
    v2.Resize(
        size=(backbone_transforms.crop_size[0], backbone_transforms.crop_size[0]),
        interpolation=backbone_transforms.interpolation,
        antialias=backbone_transforms.antialias
    ),
    v2.ToImage(),
    v2.ToDtype(torch.float32, scale=True),
    v2.Normalize(mean=backbone_transforms.mean, std=backbone_transforms.std)
])

app = FastAPI()

@app.middleware("http")
async def authenticate(request: Request, call_next):
    api_key = request.headers.get("Api-Key")
    if api_key is None or not hmac.compare_digest(api_key, API_KEY):
        return JSONResponse({
            "detail": "Invalid API key."
        }, status_code=401)

    return await call_next(request)

def run_inference(model, images):
    batch = []
    for image in images:
        with Image.open(image.file).convert("RGB") as img:
            x = transforms(img)
        
        batch.append(x)
    batch = torch.stack(batch)
    
    with torch.no_grad():
        embeddings = model(batch)
    
    return embeddings.tolist()

@app.post("/exact-match-v2")
def exact_match_v2(images: list[UploadFile]):
    return run_inference(model_v2, images)

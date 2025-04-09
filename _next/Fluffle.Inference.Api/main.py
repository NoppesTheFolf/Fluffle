from fastapi import FastAPI, UploadFile, Request
from fastapi.responses import JSONResponse
import torch
from torchvision import models
from torchvision.transforms import v2
from PIL import Image
import os
import hmac

API_KEY = os.environ["API_KEY"]

DEVICE = torch.device("cpu")
EMBEDDING_SIZE = 32

backbone = models.convnext_tiny()
backbone_in_features = backbone.classifier[2].in_features
backbone.classifier[2] = torch.nn.Identity()
model = torch.nn.Sequential(
    backbone,
    torch.nn.Dropout(p=0.5),
    torch.nn.Linear(backbone_in_features, EMBEDDING_SIZE)
)
model.load_state_dict(torch.load("model.pt", DEVICE, weights_only=True))
model.eval()

backbone_transforms = models.ConvNeXt_Tiny_Weights.IMAGENET1K_V1.transforms()
transforms = v2.Compose([
    v2.Resize(
        size=(backbone_transforms.crop_size[0], backbone_transforms.crop_size[0]),
        interpolation=backbone_transforms.interpolation,
        antialias=backbone_transforms.antialias
    ),
    v2.ToTensor(),
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

@app.post("/")
def inference(images: list[UploadFile]):
    batch = []
    for image in images:
        with Image.open(image.file).convert("RGB") as img:
            x = transforms(img)
        
        batch.append(x)
    batch = torch.stack(batch)
    
    with torch.no_grad():
        embeddings = model(batch)
    
    return embeddings.tolist()

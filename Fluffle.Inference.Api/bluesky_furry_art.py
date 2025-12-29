from fastapi import UploadFile, APIRouter
import torch
from torchvision import models
from torchvision.transforms import v2
from PIL import Image
import math
import torch.nn.functional as F

router = APIRouter()

DEVICE = torch.device("cpu")

backbone = models.convnext_tiny()
backbone_in_features = backbone.classifier[2].in_features
backbone.classifier[2] = torch.nn.Identity()
model = torch.nn.Sequential(
    backbone,
    torch.nn.Dropout(p=0.5),
    torch.nn.Linear(backbone_in_features, 2)
)
model.load_state_dict(torch.load("blueskyFurryArt.pt", DEVICE, weights_only=True))
model.eval()

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

def get_crop_boxes(img_width, img_height):
    if img_width == img_height:
        return [[0, 0, img_width, img_height]]

    max_d = max(img_width, img_height)
    min_d = min(img_width, img_height)

    crop_margin = 0.25
    effective_crop_margin = 1 - crop_margin

    crop_d = min(img_width, img_height)
    effective_crop_d = math.floor(effective_crop_margin * crop_d)

    n_crops = math.ceil(max_d / effective_crop_d)
    crop_space = n_crops * crop_d - max_d
    effective_downshift = min_d - crop_space / (n_crops - 1)

    # box: left, upper, right, lower
    boxes = []
    for i in range(n_crops):
        if img_width > img_height:
            boxes.append([i * effective_downshift, 0, min_d + i * effective_downshift, min_d])
        else:
            boxes.append([0, i * effective_downshift, min_d, min_d + i * effective_downshift])

    return boxes

def run_inference(image):
    data = []
    with Image.open(image.file) as img:
        boxes = get_crop_boxes(img.width, img.height)
        for i in range(len(boxes)):
            with img.crop(boxes[i]) as img_crop:
                data.append(transforms(img_crop.convert("RGB")))
    
    data = torch.stack(data)
    with torch.no_grad():
        y = model(data)
        probs = F.softmax(y, dim=1)
        probs_max = torch.max(probs, dim=0)
    
    return probs_max[0][0].item()

@router.post("/bluesky-furry-art")
def bluesky_furry_art(image: UploadFile):
    return run_inference(image)

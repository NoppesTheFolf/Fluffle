# Inference API

## Cheat sheet

### Running locally

```
TORCH_NUM_THREADS=1 API_KEY=abc poetry run fastapi dev
```

### Updating dependencies

```
poetry add fastapi[standard]@latest
poetry add --source pytorch.org torch@latest torchvision@latest
```

from fastapi import FastAPI, Request, Response
from starlette.status import HTTP_400_BAD_REQUEST, HTTP_401_UNAUTHORIZED
from b2 import B2ModelManager
import uvicorn
import yaml
import sys
import secrets

# Load the config
config = yaml.load(open('config.yml', 'r'), Loader=yaml.SafeLoader)

# Check if we are missing any of the required config variables
required_keys = ['host', 'port', 'api_key', 'cache_directory', 'application_key_id', 'application_key']
missing_keys = set(required_keys) - set(config.keys())
if len(missing_keys) > 0:
    raise Exception(f"You are missing the following keys in your config: {', '.join(missing_keys)}")

manager = B2ModelManager(config['cache_directory'], config['application_key_id'], config['application_key'])
app = FastAPI()

API_KEY_HEADER_NAME = 'api-key'
API_KEY = str(config['api_key'])

@app.middleware("http")
async def api_key_authentication(request: Request, call_next):
    if API_KEY_HEADER_NAME not in request.headers:
        return Response(status_code=HTTP_400_BAD_REQUEST)
    
    if not secrets.compare_digest(str(request.headers[API_KEY_HEADER_NAME]), API_KEY):
        return Response(status_code=HTTP_401_UNAUTHORIZED)
    
    response = await call_next(request)
    return response

# Load all the models defined in the config
for model_name in set(config.keys()) - set(required_keys):
    module, model = manager.model(model_name).get()

    slug = f"/{model_name.replace('_', '-')}"
    module.serve(app, slug, config[model_name], model)
    print(f'Serving {model_name} at {slug}')
    sys.stdout.flush()

@app.get('/{model_name}/{config_key}')
def get_config_key(model_name: str, config_key: str):
    return config[model_name.replace('-', '_')][config_key.replace('-', '_')]

# Start the webserver
if __name__ == '__main__':
    uvicorn.run(app, host=config['host'], port=config['port'])

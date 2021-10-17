from fastapi import FastAPI
from b2 import B2ModelManager
import uvicorn
import yaml
import sys

# Load the config
config = yaml.load(open('config.yml', 'r'), Loader=yaml.SafeLoader)

# Check if we are missing any of the required config variables
required_keys = ['host', 'port', 'cache_directory', 'application_key_id', 'application_key']
missing_keys = set(required_keys) - set(config.keys())
if len(missing_keys) > 0:
    raise Exception(f"You are missing the following keys in your config: {', '.join(missing_keys)}")

manager = B2ModelManager(config['cache_directory'], config['application_key_id'], config['application_key'])
app = FastAPI()

# Load all the models defined in the config
for model_name in set(config.keys()) - set(required_keys):
    module, model = manager.model(model_name).get()

    slug = f"/{model_name.replace('_', '-')}"
    module.serve(app, slug, config[model_name], model)
    print(f'Serving {model_name} at {slug}')
    sys.stdout.flush()

# Start the webserver
if __name__ == '__main__':
    uvicorn.run(app, host=config['host'], port=config['port'])

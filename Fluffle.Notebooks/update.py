from b2 import B2ModelManager
from b2sdk.exception import ResourceNotFound
from glob import glob
from os import path
from glob import glob
import utils
import hashlib
import yaml
import sys

base_directory = './models/'

# Load the config and create model manager
config = yaml.load(open('config.yml', 'r'), Loader=yaml.SafeLoader)
manager = B2ModelManager(config['cache_directory'], config['application_key_id'], config['application_key'])

for model_directory in glob(base_directory + '*'):
    if not path.isdir(model_directory):
        raise Exception()

    model_name = path.basename(model_directory)
    print(f'Checking if {model_name} needs to be updated...')

    file_locations = glob(path.join(model_directory, '**/*'), recursive=True)
    file_locations = map(lambda x: x.replace('\\', '/'), file_locations)
    file_locations = filter(lambda x: path.isfile(x), file_locations)
    file_locations = sorted(file_locations)
    hashes = map(lambda x: (hashlib.sha256(x.replace(model_directory.replace('\\', '/'), '').encode('ascii')).hexdigest(), utils.hash(x, hashlib.sha256)), file_locations)
    hashes_combo = ''.join(map(lambda x: ''.join(x), hashes))
    contents_sha256 = hashlib.sha256(hashes_combo.encode('ascii')).hexdigest()

    model = manager.model(model_name)
    try:
        remote_contents_sha256 = manager.bucket.get_file_info_by_name(model.archive_name).file_info['contents_sha256']
        should_upload_model = remote_contents_sha256 != contents_sha256
    except ResourceNotFound:
        should_upload_model = True
    
    print('Model needs to be updated' if should_upload_model else 'Model is already up to date')
    sys.stdout.flush()

    if should_upload_model:
        model.put(model_directory, contents_sha256)
    
    print()

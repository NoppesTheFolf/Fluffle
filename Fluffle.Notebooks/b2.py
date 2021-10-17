import tarfile
import hashlib
import b2sdk.v2 as b2
import os
from tempfile import TemporaryDirectory, TemporaryFile
import importlib
import utils


class B2ModelManager:
    def __init__(self, cache_directory, application_key_id, application_key) -> None:
        if not os.path.isdir(cache_directory):
            raise Exception('Provided cache directory location either does not exist or is not a directory.')
        
        self.cache_directory = cache_directory
        info = b2.InMemoryAccountInfo()
        b2_api = b2.B2Api(info)
        b2_api.authorize_account('production', application_key_id, application_key)
        self.bucket = b2_api.get_bucket_by_name('fluffle-models')

    def model(self, name):
        return B2Model(self.bucket, self.cache_directory, name)


class B2Model:
    def __init__(self, bucket, cache_directory, name) -> None:
        self.bucket = bucket
        self.cache_directory = cache_directory
        self.name = name
        self.archive_name = f'{self.name}.tar.gz'
        self.cache_location = os.path.join(self.cache_directory, self.archive_name)
    
    
    def put(self, location, contents_sha256):
        if not os.path.isdir(location):
            raise Exception()
        
        with TemporaryFile(delete=False) as temporary_file:
            temporary_file_location = temporary_file.name
        
        with tarfile.open(temporary_file_location, 'w|gz') as archive:
            for file_location in map(lambda x: os.path.join(location, x), os.listdir(location)):
                archive.add(file_location, arcname=os.path.basename(file_location))

        self.bucket.upload_local_file(
            local_file=temporary_file_location,
            file_name=self.archive_name,
            sha1_sum=utils.hash(temporary_file_location, hashlib.sha1),
            file_infos={
                'contents_sha256': contents_sha256,
                'sha256': utils.hash(temporary_file_location, hashlib.sha256)
            }
        )
        
        os.remove(temporary_file_location)
    
    
    def __download_model(self):
        if os.path.exists(self.cache_location):
            os.remove(self.cache_location)
        
        download = self.bucket.download_file_by_name(self.archive_name)
        download.save_to(self.cache_location)


    def get(self):
        if not os.path.exists(self.cache_location):
            self.__download_model()
        else:
            cache_hash = utils.hash(self.cache_location, hashlib.sha256)
            remote_hash = self.bucket.get_file_info_by_name(self.archive_name).file_info['sha256']
            
            if cache_hash != remote_hash:
                self.__download_model()
        
        with tarfile.open(self.cache_location, 'r') as archive:
            with TemporaryDirectory() as temporary_directory:
                archive.extractall(temporary_directory)

                load_location = os.path.join(temporary_directory, 'load.py')
                spec = importlib.util.spec_from_file_location(self.name, load_location)
                module = importlib.util.module_from_spec(spec)
                spec.loader.exec_module(module)
                
                model = module.load(temporary_directory)
        
        return (module, model)

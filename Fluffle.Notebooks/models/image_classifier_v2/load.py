import os
import json
import tensorflow as tf
from PIL import Image
import numpy as np
from more_itertools import chunked


class ImageClassifier:
    def __init__(self, tf_model, labels, length) -> None:
        self.tf_model = tf_model
        self.labels = labels
        self.length = length
    
    
    def predict(self, files, batch_size):
        results = []
        for chunk in chunked(files, batch_size):
            batch = []
            for file in chunk:
                img = Image.open(file).convert('RGB').resize((self.length, self.length), Image.NEAREST)
                img_array = tf.keras.preprocessing.image.img_to_array(img)
                img_array = tf.expand_dims(img_array, 0)
                batch.append(img_array)
            
            predictions = self.tf_model.predict(np.vstack(batch))
            predictions = map(lambda x: tf.nn.softmax(x), predictions)
            predictions = map(lambda x: map(lambda y: y.numpy().tolist(), x), predictions)
            predictions = list(map(lambda x: zip(self.labels, x), predictions))
            results.extend(predictions)
        
        return results


def load(base_dir):
    with open(os.path.join(base_dir, 'model.json')) as file:
        config = json.load(file)
    
    model = tf.keras.models.load_model(os.path.join(base_dir, 'model.hdf5'))
    return ImageClassifier(model, config['labels'], config['length'])


def serve(app, slug, config, model):
    from threading import Lock
    from typing import List
    from fastapi import File, UploadFile

    lock = Lock()
    @app.post(slug)
    def handle_request(files: List[UploadFile] = File(...)):
        with lock:
            return model.predict(map(lambda x: x.file, files), config['batch_size'])

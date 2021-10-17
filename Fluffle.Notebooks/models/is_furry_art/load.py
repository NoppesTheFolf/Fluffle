import joblib
import os


def load(base_dir):
    return joblib.load(os.path.join(base_dir, 'model.joblib'))


def serve(app, slug, config, model):
    from typing import List
    
    @app.post(slug)
    def handle_request(body: List[dict]):
        rows = list(map(lambda x: [x['anime'], x['furryArt'], x['fursuit'], x['real']], body))
        predictions = model.predict(rows)

        return list(map(lambda x: bool(x), predictions))

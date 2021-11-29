from typing import List


def load(base_dir):
    return None


def serve(app, slug, config, model):
    threshold = 0.415
    
    
    @app.post(slug)
    def handle_request(body: List[dict]):
        return list(map(lambda x: x['true'] > threshold, body))

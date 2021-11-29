import pandas as pd
import numpy as np


def load(base_dir):
    return None


def serve(app, slug, config, model):
    def simpsons(values):
        if len(values) < 2:
            return 0

        df = pd.DataFrame(values, columns=['n'])
        df['n - 1'] = df['n'] - 1
        df['n(n - 1)'] = df['n'] * df['n - 1']

        N = df['n'].sum()
        bottom = N * (N - 1)
        top = df['n(n - 1)'].sum()

        return 1 - top / bottom


    simpsons_score_threshold = 0.75
    furry_score_threshold = 0.62


    @app.post(slug)
    def handle_request(body: dict):
        if simpsons(body['artistIds']) > simpsons_score_threshold:
            return False
        
        if np.median(list(map(lambda x: x['true'], body['classes']))) < furry_score_threshold:
            return False

        return True

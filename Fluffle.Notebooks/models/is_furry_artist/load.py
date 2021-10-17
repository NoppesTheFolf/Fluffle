import joblib
import os


def load(base_dir):
    return joblib.load(os.path.join(base_dir, 'model.joblib'))


def serve(app, slug, config, model):
    import pandas as pd
    import numpy as np


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
    
    
    @app.post(slug)
    def handle_request(body: list):
        row = []
        row += pd.json_normalize(body)[['furryArt', 'real', 'fursuit', 'anime']].mean().tolist()
        
        x = list(map(lambda x: x['artistIds'], body))
        x = pd.Series(np.concatenate(x)).value_counts()
        row += [simpsons(x.values)]

        prediction = model.predict([row])
        return bool(prediction[0])

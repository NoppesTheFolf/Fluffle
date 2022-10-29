poetry export --without-hashes -o requirements.txt
python -m venv env
source env/Scripts/activate
pip install -r requirements.txt

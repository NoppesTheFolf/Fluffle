FROM python:3.9-bullseye
WORKDIR /app

# Restore dependencies in a separate layer
COPY requirements.txt ./
RUN pip3 install --no-cache-dir --upgrade pip && \
    pip3 install --no-cache-dir -r requirements.txt

# Copy everything else and start the API
COPY . .
CMD python3 api.py

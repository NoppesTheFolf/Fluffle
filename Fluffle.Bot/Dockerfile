FROM python:3.10-buster
WORKDIR /app

RUN apt-get update && apt-get install -y libvips-dev

COPY ./Fluffle.Bot/ .
RUN pip3 install --no-cache-dir --upgrade pip && \
    pip3 install --no-cache-dir -r requirements.txt

ENTRYPOINT python3 -m main

FROM python:3.10-buster
WORKDIR /app

RUN apt-get update && apt-get install -y libvips-dev

COPY ./ .
RUN pip3 install -r requirements.txt

ENTRYPOINT python3 -m main

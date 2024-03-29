FROM python:3.9-bullseye
WORKDIR /app

# Install Bazel
RUN apt-get update && \
    apt-get install -y apt-transport-https curl gnupg && \
    curl -fsSL https://bazel.build/bazel-release.pub.gpg | gpg --dearmor > bazel.gpg && \
    mv bazel.gpg /etc/apt/trusted.gpg.d/ && \
    echo "deb [arch=amd64] https://storage.googleapis.com/bazel-apt stable jdk1.8" | tee /etc/apt/sources.list.d/bazel.list && \
    apt-get update && \
    apt-get install -y bazel-3.7.2 && \
    ln -s /usr/bin/bazel-3.7.2 /usr/bin/bazel && \
    apt-get purge -y curl

# Restore dependencies in a separate layer
COPY ./Fluffle.Notebooks/requirements.txt ./requirements.txt
RUN pip3 install --no-cache-dir --upgrade pip && \
    pip3 install --no-cache-dir -r requirements.txt

ARG MEM_LIMIT=4096

# Clone tensorflow
RUN apt-get update && \
    apt-get install -y git && \
    git clone https://github.com/tensorflow/tensorflow && \
    cd tensorflow && \
    git checkout v2.7.0 && \
    ./configure && \
    bazel build -c opt --copt=-mfma --copt=-mavx --copt=-mavx2 --copt=-msse4.1 --copt=-msse4.2 --verbose_failures --local_ram_resources=$MEM_LIMIT //tensorflow/tools/pip_package:build_pip_package && \
    ./bazel-bin/tensorflow/tools/pip_package/build_pip_package /tmp/tensorflow_pkg && \
    pip3 install --no-cache-dir /tmp/tensorflow_pkg/tensorflow-2.7.0-cp39-cp39-linux_x86_64.whl && \
    cd .. && \
    apt-get purge -y git && \
    rm -rf tensorflow /tmp/tensorflow_pkg /root/.cache 

# Copy everything else and start the API
COPY ./Fluffle.Notebooks/*.py ./
CMD python3 api.py

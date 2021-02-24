FROM debian:buster
WORKDIR /vips-build

# Install utilities
RUN apt-get update && apt-get install -y curl apt-transport-https sudo

# Script dependencies
RUN apt-get update && \
    apt-get install -y wget git curl apt-transport-https

# Build dependencies
RUN apt-get update && \
    apt-get install -y build-essential pkg-config libglib2.0-dev gtk-doc-tools libexpat1-dev

# EXIF support
RUN apt-get install -y libexif-dev

# ICC profile support
# FFTW (Fast Fourier Transforms) 
# Orc
RUN apt-get update && apt-get -y install cmake liblcms2-dev libfftw3-dev liborc-0.4-dev

RUN wget -qq https://github.com/glennrp/libpng/archive/v1.6.37.tar.gz && \
    tar xf v1.6.37.tar.gz && \
    cd libpng-1.6.37 && \
    cmake -G"Unix Makefiles" -DCMAKE_BUILD_TYPE=Release && \
    make -j$(nproc) && \
    make install

RUN wget -qq https://github.com/mozilla/mozjpeg/archive/v4.0.0.tar.gz && \
    tar xf v4.0.0.tar.gz && \
    cd mozjpeg-4.0.0 && \
    cmake -G"Unix Makefiles" -DCMAKE_BUILD_TYPE=Release && \
    make -j$(nproc) && \
    make install

# mozjpeg installs to /opt/mozjpeg ... we need that on PKG_CONFIG_PATH so
# that libvips configure can find it
ENV PKG_CONFIG_PATH /opt/mozjpeg/lib64/pkgconfig
ENV LD_LIBRARY_PATH="/opt/mozjpeg/lib64:${LD_LIBRARY_PATH}"

RUN wget -qq https://deac-ams.dl.sourceforge.net/project/giflib/giflib-5.2.1.tar.gz && \
    tar xf giflib-5.2.1.tar.gz && \
    cd giflib-5.2.1 && \
    make -j$(nproc) && \
    make install

RUN wget -qq https://storage.googleapis.com/downloads.webmproject.org/releases/webp/libwebp-1.1.0.tar.gz && \
    tar xf libwebp-1.1.0.tar.gz && \
    cd libwebp-1.1.0 && \
    ./configure --enable-libwebpmux --enable-libwebpdecoder --enable-libwebpextras && \
    make -j$(nproc) && \
    make install

RUN git clone https://aomedia.googlesource.com/aom && \
    cd aom && \
    git checkout v2.0.1 && \
    mkdir build/linux && cd build/linux && cmake -G"Unix Makefiles" -DCMAKE_BUILD_TYPE=Release -DAOM_TARGET_CPU=generic -DCMAKE_POSITION_INDEPENDENT_CODE=TRUE ../../ && \
    make -j$(nproc) && \
    make install

RUN wget -qq https://github.com/strukturag/libheif/releases/download/v1.10.0/libheif-1.10.0.tar.gz && \
    tar xf libheif-1.10.0.tar.gz && \
    cd libheif-1.10.0 && \
    ./configure --disable-examples --disable-go && \
    make -j$(nproc) && \
    make install

RUN wget -qq https://github.com/libvips/libvips/releases/download/v8.10.5/vips-8.10.5.tar.gz && \
    tar xf vips-8.10.5.tar.gz && \
    cd vips-8.10.5 && \
    CFLAGS=-O3 CXXFLAGS=-O3 ./configure && \
    make -j$(nproc) && \
    make install && \
    ldconfig

# Install .NET Core SDK
RUN curl -sL https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb -o packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt-get update && apt-get install -y dotnet-sdk-5.0

WORKDIR /fluffle/build
RUN rm -rf /vips-build

# Copy csproj and restore as distinct layers
COPY ./**/*.csproj ./
RUN ls | while read line; do mkdir $(basename $line .csproj) && mv $line $(basename $line .csproj); done
COPY ./Fluffle.sln .
RUN dotnet restore

# Copy everything else
COPY . ./

# Compile interop library 
RUN cd ./Fluffle.Vips && ./FluffleVips.sh

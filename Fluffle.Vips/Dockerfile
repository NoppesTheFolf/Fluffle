FROM debian:bullseye
WORKDIR /vips-build

# Install utilities
RUN apt-get update && \
    apt-get install -y curl apt-transport-https sudo && \
    rm -rf /var/lib/apt/lists/*

# Script dependencies
RUN apt-get update && \
    apt-get install -y wget git curl apt-transport-https && \
    rm -rf /var/lib/apt/lists/*

# Build dependencies
RUN apt-get update && \
    apt-get install -y build-essential pkg-config libglib2.0-dev libexpat1-dev meson ninja-build libgirepository1.0-dev && \
    rm -rf /var/lib/apt/lists/*

# EXIF support
RUN apt-get update && \
    apt-get install -y libexif-dev && \
    rm -rf /var/lib/apt/lists/*

# ICC profile support
# FFTW (Fast Fourier Transforms) 
# Orc
RUN apt-get update && \
    apt-get -y install cmake liblcms2-dev libfftw3-dev liborc-0.4-dev && \
    rm -rf /var/lib/apt/lists/*

# mozjpeg installs to /opt/mozjpeg ... we need that on PKG_CONFIG_PATH so
# that libvips configure can find it
ENV PKG_CONFIG_PATH="/opt/mozjpeg/lib64/pkgconfig"
ENV LD_LIBRARY_PATH="/opt/mozjpeg/lib64:${LD_LIBRARY_PATH}"

RUN wget -qq https://github.com/glennrp/libpng/archive/v1.6.40.tar.gz && \
    tar xf v1.6.40.tar.gz && \
    cd libpng-1.6.40 && \
    cmake -G"Unix Makefiles" -DCMAKE_BUILD_TYPE=Release && \
    make -j$(nproc) && \
    make install && \
    cd .. && \
    rm -rf v1.6.40.tar.gz libpng-1.6.40

RUN wget -qq https://github.com/randy408/libspng/archive/refs/tags/v0.7.4.tar.gz && \
    tar xf v0.7.4.tar.gz && \
    cd libspng-0.7.4 && \
    cmake -G"Unix Makefiles" -DCMAKE_BUILD_TYPE=Release && \
    make -j$(nproc) && \
    make install && \
    cd .. && \
    rm -rf v0.7.4.tar.gz libspng-0.7.4

RUN wget -qq https://github.com/mozilla/mozjpeg/archive/v4.1.3.tar.gz && \
    tar xf v4.1.3.tar.gz && \
    cd mozjpeg-4.1.3 && \
    cmake -G"Unix Makefiles" -DCMAKE_BUILD_TYPE=Release && \
    make -j$(nproc) && \
    make install && \
    cd .. && \
    rm -rf v4.1.3.tar.gz mozjpeg-4.1.3

RUN wget -qq https://deac-fra.dl.sourceforge.net/project/giflib/giflib-5.2.1.tar.gz && \
    tar xf giflib-5.2.1.tar.gz && \
    cd giflib-5.2.1 && \
    make -j$(nproc) && \
    make install && \
    cd .. && \
    rm -rf giflib-5.2.1.tar.gz giflib-5.2.1

RUN wget -qq https://storage.googleapis.com/downloads.webmproject.org/releases/webp/libwebp-1.3.1.tar.gz && \
    tar xf libwebp-1.3.1.tar.gz && \
    cd libwebp-1.3.1 && \
    ./configure --enable-libwebpmux --enable-libwebpdecoder --enable-libwebpextras && \
    make -j$(nproc) && \
    make install && \
    cd .. && \
    rm -rf libwebp-1.3.1.tar.gz libwebp-1.3.1

RUN git clone https://aomedia.googlesource.com/aom && \
    cd aom && \
    git checkout v3.6.1 && \
    mkdir build/linux && cd build/linux && cmake -G"Unix Makefiles" -DCMAKE_BUILD_TYPE=Release -DAOM_TARGET_CPU=generic -DCMAKE_POSITION_INDEPENDENT_CODE=TRUE ../../ && \
    make -j$(nproc) && \
    make install && \
    cd ../../.. && \
    rm -rf aom

RUN wget -qq https://github.com/strukturag/libde265/releases/download/v1.0.12/libde265-1.0.12.tar.gz && \
    tar xf libde265-1.0.12.tar.gz && \
    cd libde265-1.0.12 && \
    cmake -G"Unix Makefiles" -DCMAKE_BUILD_TYPE=Release && \
    make -j$(nproc) && \
    make install && \
    cd .. && \
    rm -rf libde265-1.0.12.tar.gz libde265-1.0.12

RUN wget -qq https://github.com/strukturag/libheif/releases/download/v1.16.2/libheif-1.16.2.tar.gz && \
    tar xf libheif-1.16.2.tar.gz && \
    cd libheif-1.16.2 && \
    mkdir build && \
    cd build && \
    cmake --preset=release .. && \
    make -j$(nproc) && \
    make install && \
    cd ../.. && \
    rm -rf libheif-1.16.2.tar.gz libheif-1.16.2

RUN wget -qq https://github.com/libvips/libvips/releases/download/v8.14.2/vips-8.14.2.tar.xz && \
    tar xf vips-8.14.2.tar.xz && \
    cd vips-8.14.2 && \
    meson setup --buildtype release build && \
    cd build && \
    meson compile && \
    meson install && \
    cd ../.. && \
    rm -rf vips-8.14.2.tar.xz vips-8.14.2 && \
    ldconfig

# Install .NET Core SDK
RUN curl -sL https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -o packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y dotnet-sdk-8.0 && \
    rm -rf /var/lib/apt/lists/*

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
RUN cd ./Fluffle.Vips && \
    g++ -O3 -shared -o LibFluffleVips.so -Wall -fPIC FluffleVips.cpp `pkg-config vips-cpp --cflags --libs`

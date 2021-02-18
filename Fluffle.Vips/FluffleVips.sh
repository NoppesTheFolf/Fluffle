#!/bin/bash

g++ -O3 -shared -o LibFluffleVips.so -Wall -fPIC FluffleVips.cpp `pkg-config vips-cpp --cflags --libs`

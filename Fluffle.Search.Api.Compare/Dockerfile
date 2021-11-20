FROM rust:1.56 as build-env
WORKDIR /src

COPY . .
RUN RUSTFLAGS="-C target-cpu=native" cargo install --path .

FROM debian:bullseye-slim
WORKDIR /fluffle

COPY --from=build-env /usr/local/cargo/bin/fluffle-compare /usr/local/bin/fluffle-compare
CMD ["fluffle-compare"]

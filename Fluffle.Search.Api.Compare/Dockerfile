FROM rust:1.65 as build-env
WORKDIR /src

COPY ./Fluffle.Search.Api.Compare/ .
RUN RUSTFLAGS="-C target-feature=+popcnt" cargo install --path .

FROM debian:bullseye-slim
WORKDIR /fluffle

COPY --from=build-env /usr/local/cargo/bin/fluffle-compare /usr/local/bin/fluffle-compare
CMD ["fluffle-compare"]

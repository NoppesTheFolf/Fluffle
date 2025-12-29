# base stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble AS base

RUN apt-get update && apt-get install -y libvips

USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /build

COPY ./**/*.csproj ./
RUN ls | while read line; do mkdir $(basename $line .csproj) && mv $line $(basename $line .csproj); done
COPY ./Fluffle.sln ./
RUN dotnet restore

COPY . ./
RUN dotnet build --no-restore --configuration Release
RUN dotnet publish --no-build --configuration Release --output publish ./Fluffle.Imaging.Api

FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble
WORKDIR /app

RUN apt-get update && apt-get install -y libvips

COPY --from=build --chown=$APP_UID /build/publish .

USER $APP_UID
ENTRYPOINT [ "dotnet", "Fluffle.Imaging.Api.dll" ]

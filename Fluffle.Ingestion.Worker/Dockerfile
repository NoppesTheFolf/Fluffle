FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

COPY ./**/*.csproj ./
RUN ls | while read line; do mkdir $(basename $line .csproj) && mv $line $(basename $line .csproj); done
COPY ./Fluffle.sln ./
RUN dotnet restore

COPY . ./
RUN dotnet build --no-restore --configuration Release
RUN dotnet publish --no-build --configuration Release --output publish ./Fluffle.Ingestion.Worker

FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled
WORKDIR /app

COPY --from=build /build/publish .
ENTRYPOINT [ "dotnet", "Fluffle.Ingestion.Worker.dll" ]

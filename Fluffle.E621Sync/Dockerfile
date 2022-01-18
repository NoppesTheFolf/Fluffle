FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /fluffle

# Copy csproj and restore as distinct layers
COPY ./**/*.csproj ./
RUN ls | while read line; do mkdir $(basename $line .csproj) && mv $line $(basename $line .csproj); done
COPY ./Fluffle.sln .
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o publish ./Fluffle.E621Sync

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /fluffle
COPY --from=build-env /fluffle/publish .
ENTRYPOINT ["dotnet", "Noppes.Fluffle.E621Sync.dll"]

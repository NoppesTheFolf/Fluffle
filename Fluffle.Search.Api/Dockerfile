FROM fluffle-vips:latest

# Publish web application
RUN dotnet publish -c Release -o publish ./Fluffle.Search.Api && \
    cd .. && \
    mv ./build/publish/* . && \
    rm -rf ./build

WORKDIR /fluffle
ENTRYPOINT ["dotnet", "Noppes.Fluffle.Search.Api.dll"]

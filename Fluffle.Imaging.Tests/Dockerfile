FROM fluffle-vips:latest

RUN dotnet publish -c Release -o publish ./Fluffle.Imaging.Tests && \
    cd .. && \
    mv ./build/publish/* . && \
    rm -rf ./build

WORKDIR /fluffle
ENTRYPOINT ["dotnet", "Noppes.Fluffle.Imaging.Tests.dll"]

FROM fluffle-vips:latest

RUN dotnet publish -c Release -o publish ./Fluffle.Bot && \
    cd .. && \
    mv ./build/publish/* . && \
    rm -rf ./build

WORKDIR /fluffle
ENTRYPOINT ["dotnet", "Noppes.Fluffle.Bot.dll"]

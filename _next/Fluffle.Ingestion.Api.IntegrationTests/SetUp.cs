using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using System.Net;
using Testcontainers.MongoDb;

namespace Fluffle.Ingestion.Api.IntegrationTests;

[SetUpFixture]
public class SetUp
{
    private static readonly INetwork Network = new NetworkBuilder()
        .WithName("fluffle-ingestion-api-integration-tests")
        .Build();

    private static readonly IFutureDockerImage ApiImage = new ImageFromDockerfileBuilder()
        .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
        .WithDockerfile("./Fluffle.Ingestion.Api/Dockerfile")
        .Build();

    private static readonly IContainer ApiContainer = new ContainerBuilder()
        .WithImage(ApiImage)
        .WithNetwork(Network)
        .WithPortBinding(1080, 8080)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(x => x.ForPort(8080).ForStatusCode(HttpStatusCode.Unauthorized)))
        .WithBindMount(Path.Join(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, "Fluffle.Ingestion.Api/appsettings.Integration.json"), "/app/appsettings.json", AccessMode.ReadOnly)
        .Build();

    private static readonly MongoDbContainer MongoContainer = new MongoDbBuilder()
        .WithImage("mongo:8.0")
        .WithNetwork(Network)
        .WithHostname("mongo")
        .WithUsername("root")
        .WithPassword("noo4aeNai3ohthah3rohmie9zi7veph9")
        .Build();

    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        await Network.CreateAsync();

        var startMongoTask = MongoContainer.StartAsync();

        await ApiImage.CreateAsync();
        await ApiContainer.StartAsync();

        await startMongoTask;
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await ApiContainer.DisposeAsync();
        await ApiImage.DisposeAsync();
        await MongoContainer.DisposeAsync();
        await Network.DisposeAsync();
    }
}

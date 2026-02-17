using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using System.Net;
using Testcontainers.MongoDb;
using Testcontainers.Qdrant;

namespace Fluffle.Vector.Api.IntegrationTests;

[SetUpFixture]
public class SetUp
{
    private static readonly INetwork Network = new NetworkBuilder()
        .WithName("fluffle-vector-api-integration-tests")
        .Build();

    private static readonly IFutureDockerImage ApiImage = new ImageFromDockerfileBuilder()
        .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
        .WithDockerfile("./Fluffle.Vector.Api/Dockerfile")
        .Build();

    private static readonly IContainer ApiContainer = new ContainerBuilder(ApiImage)
        .WithNetwork(Network)
        .WithPortBinding(53966, 8080)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(x => x.ForPort(8080).ForStatusCode(HttpStatusCode.Unauthorized)))
        .WithBindMount(Path.Join(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, "Fluffle.Vector.Api/appsettings.Integration.json"), "/app/appsettings.json", AccessMode.ReadOnly)
        .Build();

    private static readonly MongoDbContainer MongoContainer = new MongoDbBuilder("mongo:8.0")
        .WithNetwork(Network)
        .WithHostname("mongo")
        .WithUsername("root")
        .WithPassword("noo4aeNai3ohthah3rohmie9zi7veph9")
        .Build();

    private static readonly QdrantContainer QdrantContainer = new QdrantBuilder("qdrant/qdrant:v1.15.5")
        .WithNetwork(Network)
        .WithHostname("qdrant")
        .WithApiKey("eep3waharah7chah4tohshe4Aephohsh")
        .Build();

    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        await Network.CreateAsync();

        var startMongoTask = MongoContainer.StartAsync();
        var startQdrantTask = QdrantContainer.StartAsync();

        await ApiImage.CreateAsync();
        await ApiContainer.StartAsync();

        await Task.WhenAll(startMongoTask, startQdrantTask);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await ApiContainer.DisposeAsync();
        await ApiImage.DisposeAsync();
        await QdrantContainer.DisposeAsync();
        await MongoContainer.DisposeAsync();
        await Network.DisposeAsync();
    }
}

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using System.Net;

namespace Fluffle.Inference.Api.IntegrationTests;

[SetUpFixture]
public class SetUp
{
    private static readonly IFutureDockerImage ApiImage = new ImageFromDockerfileBuilder()
        .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
        .WithDockerfile("./Fluffle.Inference.Api/Dockerfile")
        .Build();

    private static readonly IContainer ApiContainer = new ContainerBuilder()
        .WithImage(ApiImage)
        .WithPortBinding(1080, 8000)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(x => x.ForPort(8000).ForStatusCode(HttpStatusCode.Unauthorized)))
        .WithBindMount(Path.Join(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, "Fluffle.Inference.Api/exactMatchV2.pt"), "/app/exactMatchV2.pt", AccessMode.ReadOnly)
        .WithEnvironment("API_KEY", "iesheeguThu4Kee4ahthaek9zeetinei")
        .WithEnvironment("TORCH_NUM_THREADS", "1")
        .WithEnvironment("FASTAPI_WORKERS", "1")
        .Build();

    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        await ApiImage.CreateAsync();
        await ApiContainer.StartAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await ApiContainer.DisposeAsync();
        await ApiImage.DisposeAsync();
    }
}

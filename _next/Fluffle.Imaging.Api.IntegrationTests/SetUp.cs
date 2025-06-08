using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using System.Net;

namespace Fluffle.Imaging.Api.IntegrationTests;

[SetUpFixture]
public class SetUp
{
    private static readonly IFutureDockerImage ApiImage = new ImageFromDockerfileBuilder()
        .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
        .WithDockerfile("./Fluffle.Imaging.Api/Dockerfile")
        .Build();

    private static readonly IContainer ApiContainer = new ContainerBuilder()
        .WithImage(ApiImage)
        .WithPortBinding(1080, 8080)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(x => x.ForPort(8080).ForStatusCode(HttpStatusCode.Unauthorized)))
        .WithBindMount(Path.Join(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, "Fluffle.Imaging.Api/appsettings.Integration.json"), "/app/appsettings.json", AccessMode.ReadOnly)
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

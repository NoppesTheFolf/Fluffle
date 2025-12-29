using DotNet.Testcontainers.Builders;
using Fluffle.Imaging.Api.Client;
using Fluffle.Imaging.Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Text.Json;

namespace Fluffle.Imaging.Api.IntegrationTests;

public class Tests
{
    private ServiceProvider _serviceProvider;
    private IImagingApiClient _imagingApiClient;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _serviceProvider = new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder()
                .AddInMemoryCollection([
                    new KeyValuePair<string, string?>("ImagingApiClient:Url", "http://127.0.0.1:51502"),
                    new KeyValuePair<string, string?>("ImagingApiClient:ApiKey", "ooghahfeig9yiThaghozu4no7kae9Pho")
                ])
                .Build())
            .AddImagingApiClient()
            .BuildServiceProvider();

        _imagingApiClient = _serviceProvider.GetRequiredService<IImagingApiClient>();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await _serviceProvider.DisposeAsync();
    }

    [TestCase("ColorSpaceCmyk", "jpg", 300, 95, true)]
    [TestCase("Metadata", "jpg", 300, 95, true)]
    [TestCase("OrientationLeftBottom", "jpg", 300, 95, true)]
    [TestCase("Jpeg", "jpg", 500, 75, false)]
    [TestCase("WebP", "webp", 600, 75, true)]
    [TestCase("Png", "png", 400, 75, true)]
    [TestCase("Gif", "gif", 300, 75, true)]
    public async Task Test01_ValidImage_ReturnsExpected(string name, string inputExtension, int size, int quality, bool calculateCenter)
    {
        var testCasePath = Path.Join(CommonDirectoryPath.GetProjectDirectory().DirectoryPath, "TestCases", name);
        var inputImagePath = Path.Join(testCasePath, $"input.{inputExtension}");
        var inputImageMetadataPath = Path.Join(testCasePath, "input.json");

        await using var inputImageStream1 = File.OpenRead(inputImagePath);
        var expectedInputImageMetadataJson = await File.ReadAllTextAsync(inputImageMetadataPath);
        var expectedInputImageMetadata = JsonSerializer.Deserialize<ImageMetadataModel>(expectedInputImageMetadataJson, JsonSerializerOptions.Web);
        var inputImageMetadata = await _imagingApiClient.GetMetadataAsync(inputImageStream1);
        inputImageMetadata.ShouldBeEquivalentTo(expectedInputImageMetadata);

        var expectedImagePath = Path.Join(testCasePath, "expected.jpg");
        var expectedImageMetadataPath = Path.Join(testCasePath, "expected.json");

        await using var inputImageStream2 = File.OpenRead(inputImagePath);
        var (actualImage, actualImageMetadata) = await _imagingApiClient.CreateThumbnailAsync(inputImageStream2, size, quality, calculateCenter);

        var expectedImage = await File.ReadAllBytesAsync(expectedImagePath);
        actualImage.ShouldBe(expectedImage);

        var expectedImageMetadataJson = await File.ReadAllTextAsync(expectedImageMetadataPath);
        var expectedImageMetadata = JsonSerializer.Deserialize<ImageMetadataModel>(expectedImageMetadataJson, JsonSerializerOptions.Web);
        actualImageMetadata.ShouldBeEquivalentTo(expectedImageMetadata);
    }

    [Test]
    public async Task Test02_RandomBytes_ReturnsUnsupportedImage()
    {
        var buffer = new byte[4096];
        Random.Shared.NextBytes(buffer);

        await using var bufferStream1 = new MemoryStream(buffer);
        var actMetadata = _imagingApiClient.GetMetadataAsync(bufferStream1);
        var eMetadata = await actMetadata.ShouldThrowAsync<ImagingApiException>();
        eMetadata.Code.ShouldBe(ImagingErrorCode.UnsupportedImage);

        await using var bufferStream2 = new MemoryStream(buffer);
        var actThumbnail = _imagingApiClient.CreateThumbnailAsync(bufferStream2, size: 300, quality: 95, calculateCenter: true);
        var eThumbnail = await actThumbnail.ShouldThrowAsync<ImagingApiException>();
        eThumbnail.Code.ShouldBe(ImagingErrorCode.UnsupportedImage);
    }
}

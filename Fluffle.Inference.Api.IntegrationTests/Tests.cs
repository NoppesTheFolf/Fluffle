using DotNet.Testcontainers.Builders;
using Fluffle.Inference.Api.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Fluffle.Inference.Api.IntegrationTests;

public class Tests
{
    private ServiceProvider _serviceProvider;
    private IInferenceApiClient _inferenceApiClient;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _serviceProvider = new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder()
                .AddInMemoryCollection([
                    new KeyValuePair<string, string?>("InferenceApiClient:Url", "http://127.0.0.1:51408"),
                    new KeyValuePair<string, string?>("InferenceApiClient:ApiKey", "iesheeguThu4Kee4ahthaek9zeetinei")
                ])
                .Build())
            .AddInferenceApiClient()
            .BuildServiceProvider();

        _inferenceApiClient = _serviceProvider.GetRequiredService<IInferenceApiClient>();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await _serviceProvider.DisposeAsync();
    }

    [Test]
    public async Task Test01_RunExactMatchV2Inference_ReturnsSingleVector()
    {
        var imagePath = Path.Join(CommonDirectoryPath.GetProjectDirectory().DirectoryPath, "image.jpg");
        await using var imageStream = File.OpenRead(imagePath);

        var vectors = await _inferenceApiClient.ExactMatchV2Async([imageStream]);

        vectors.ShouldNotBeNull();
        vectors.Length.ShouldBe(1);
        vectors[0].Length.ShouldBe(64);
        vectors[0].ShouldBe([
            20.44205093383789f,
            -35.510746002197266f,
            17.37627601623535f,
            -22.706541061401367f,
            10.568754196166992f,
            4.724219799041748f,
            -38.04255676269531f,
            -5.308432579040527f,
            -12.498006820678711f,
            1.0068868398666382f,
            -19.32323455810547f,
            -16.794960021972656f,
            -12.255047798156738f,
            -4.323366165161133f,
            -9.402094841003418f,
            -3.3522250652313232f,
            9.31024169921875f,
            -9.498909950256348f,
            9.422588348388672f,
            0.13514086604118347f,
            37.02531051635742f,
            7.1200175285339355f,
            4.605930805206299f,
            -14.696440696716309f,
            -13.544525146484375f,
            19.026609420776367f,
            -1.5575929880142212f,
            42.60084915161133f,
            -27.44767951965332f,
            5.928417682647705f,
            8.650282859802246f,
            -11.832505226135254f,
            -22.30732536315918f,
            -13.730047225952148f,
            -17.237337112426758f,
            -18.74934196472168f,
            45.382625579833984f,
            5.541863918304443f,
            19.383792877197266f,
            -5.308034896850586f,
            -9.377483367919922f,
            20.358009338378906f,
            -9.406057357788086f,
            7.350724697113037f,
            -7.19285249710083f,
            23.08005714416504f,
            28.674118041992188f,
            3.6858932971954346f,
            -17.933122634887695f,
            -20.40389633178711f,
            -18.18514633178711f,
            -2.6533777713775635f,
            -1.8226598501205444f,
            -12.305961608886719f,
            36.72810363769531f,
            14.573135375976562f,
            8.653170585632324f,
            12.86595344543457f,
            8.570107460021973f,
            -33.31332778930664f,
            8.585821151733398f,
            -21.74779510498047f,
            -10.53797435760498f,
            28.262622833251953f
        ], 0.0001f);
    }

    [Test]
    public async Task Test02_RunBlueskyFurryArtInference_ReturnsSinglePrediction()
    {
        var imagePath = Path.Join(CommonDirectoryPath.GetProjectDirectory().DirectoryPath, "image.jpg");
        await using var imageStream = File.OpenRead(imagePath);

        var prediction = await _inferenceApiClient.BlueskyFurryArtAsync(imageStream);

        prediction.ShouldBe(0.6095377802848816f);
    }
}

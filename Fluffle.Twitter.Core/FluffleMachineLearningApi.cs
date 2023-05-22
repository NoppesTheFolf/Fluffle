using Flurl.Http;
using Noppes.Fluffle.Http;

namespace Noppes.Fluffle.Twitter.Core;

public class FurryArtPredictionModel
{
    public double False { get; set; }

    public double True { get; set; }
}

public interface IFluffleMachineLearningApiClient
{
    Task<IList<double>> GetFurryArtPredictionsAsync(IEnumerable<Stream> streams);

    Task<double> GetFurryArtistPredictionAsync(ICollection<double> furryArtPredictions);

    Task<bool> VerifyImageAsync(Stream stream);
}

public class FluffleMachineLearningApiClient : ApiClient, IFluffleMachineLearningApiClient
{
    private const string DummyFileName = "dummy";

    private readonly string _apiKey;

    public FluffleMachineLearningApiClient(string baseUrl, string apiKey) : base(baseUrl)
    {
        _apiKey = apiKey;
    }

    public async Task<IList<double>> GetFurryArtPredictionsAsync(IEnumerable<Stream> streams)
    {
        var response = await Request("furry-art")
            .AcceptJson()
            .PostMultipartAsync(x =>
            {
                foreach (var stream in streams)
                {
                    x.AddFile("files", stream, DummyFileName);
                }
            });

        var result = await response.GetJsonAsync<IList<FurryArtPredictionModel>>();
        return result.Select(x => x.True).ToList();
    }

    public async Task<double> GetFurryArtistPredictionAsync(ICollection<double> furryArtPredictions)
    {
        var response = await Request("furry-artist")
            .AcceptJson()
            .PostJsonAsync(furryArtPredictions);

        var result = await response.GetJsonAsync<double>();
        return result;
    }

    public async Task<bool> VerifyImageAsync(Stream stream)
    {
        try
        {
            await Request("verify-image")
                .AcceptJson()
                .PostMultipartAsync(x =>
                {
                    x.AddFile("file", stream, DummyFileName);
                });
        }
        catch (FlurlHttpException e)
        {
            if (e.StatusCode == 400)
                return false;

            throw;
        }


        return true;
    }

    public override IFlurlRequest Request(params object[] urlSegments)
    {
        return base.Request(urlSegments)
            .WithHeader("Api-Key", _apiKey);
    }
}

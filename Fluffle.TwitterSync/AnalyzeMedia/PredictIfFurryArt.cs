using Noppes.Fluffle.Http;
using Noppes.Fluffle.Utils;
using System.Threading.Tasks;

namespace Noppes.Fluffle.TwitterSync.AnalyzeMedia;

public class PredictIfFurryArt : Consumer<AnalyzeMediaData>
{
    private readonly IPredictionClient _predictionClient;

    public PredictIfFurryArt(IPredictionClient predictionClient)
    {
        _predictionClient = predictionClient;
    }

    public override async Task<AnalyzeMediaData> ConsumeAsync(AnalyzeMediaData data)
    {
        data.IsFurryArt = await HttpResiliency.RunAsync(() => _predictionClient.IsFurryArtAsync(data.Classes));

        return data;
    }
}

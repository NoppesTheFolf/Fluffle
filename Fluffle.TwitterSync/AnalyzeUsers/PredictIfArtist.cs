using Noppes.Fluffle.Utils;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using Noppes.Fluffle.Http;

namespace Noppes.Fluffle.TwitterSync.AnalyzeUsers
{
    public class PredictIfArtist : Consumer<AnalyzeUserData>
    {
        private readonly IPredictionClient _predictionClient;

        public PredictIfArtist(IPredictionClient predictionClient)
        {
            _predictionClient = predictionClient;
        }

        public override async Task<AnalyzeUserData> ConsumeAsync(AnalyzeUserData data)
        {
            var scores = data.Classes.Zip(data.BestMatches, (prediction, bestMatch) =>
            {
                return new AnalyzeScore
                {
                    FurryArt = prediction[ClassificationClass.FurryArt],
                    Fursuit = prediction[ClassificationClass.Fursuit],
                    Real = prediction[ClassificationClass.Real],
                    Anime = prediction[ClassificationClass.Anime],
                    ArtistIds = bestMatch == null || bestMatch.Credits.Count > 1 ? Array.Empty<int>() : bestMatch.Credits.Select(c => c.Id).ToArray()
                };
            });

            data.IsFurryArtist = await HttpResiliency.RunAsync(() => _predictionClient.IsFurryArtistAsync(scores));
            Log.Information("Is @{username} a furry artist? {value}", data.Username, data.IsFurryArtist);

            return data;
        }
    }
}

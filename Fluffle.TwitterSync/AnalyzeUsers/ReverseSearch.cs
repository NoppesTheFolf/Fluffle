using Noppes.Fluffle.Http;
using Noppes.Fluffle.Utils;
using SerilogTimings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.TwitterSync.AnalyzeUsers
{
    public class ReverseSearch : Consumer<AnalyzeUserData>
    {
        private readonly IReverseSearchClient _reverseClient;

        public ReverseSearch(IReverseSearchClient reverseClient)
        {
            _reverseClient = reverseClient;
        }

        public override async Task<AnalyzeUserData> ConsumeAsync(AnalyzeUserData data)
        {
            // Reverse search images using Fluffle
            data.BestMatches = new List<FluffleResult>();

            foreach (var openStream in data.OpenStreams)
            {
                using var _ = Operation.Time("Reverse searching an image for user @{username}", data.Username);

                // Reverse search on Fluffle using only e621 as a source as that will always have the artist attached
                var searchResult = await HttpResiliency.RunAsync(() => _reverseClient.ReverseSearchAsync(openStream, true, 8, FlufflePlatform.E621));
                var bestMatch = searchResult.Results
                    .Where(r => r.Match != FluffleMatch.Unlikely)
                    .OrderByDescending(r => r.Match)
                    .ThenByDescending(r => r.Score)
                    .FirstOrDefault();

                data.BestMatches.Add(bestMatch);
            }

            return data;
        }
    }
}

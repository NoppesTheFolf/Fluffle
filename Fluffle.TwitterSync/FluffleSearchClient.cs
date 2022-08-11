using Flurl.Http;
using Noppes.Fluffle.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Noppes.Fluffle.TwitterSync
{
    // Eventually this should become its own NuGet package backed by an open source repository
    public enum FlufflePlatform
    {
        E621,
        FurryNetwork,
        FurAffinity
    }

    public class FluffleResponse
    {
        public FluffleStats Stats { get; set; }

        public List<FluffleResult> Results { get; set; }
    }

    public class FluffleStats
    {
        public int Count { get; set; }

        public int ElapsedMilliseconds { get; set; }
    }

    public enum FluffleMatch
    {
        Unlikely = 0,
        TossUp = 1,
        Alternative = 2,
        Exact = 3
    }

    public class FluffleResult
    {
        public float Score { get; set; }

        public FluffleMatch Match { get; set; }

        public class Credit
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public List<Credit> Credits { get; set; }
    }

    public interface IReverseSearchClient
    {
        public Task<FluffleResponse> ReverseSearchAsync(Func<Stream> openStream, bool includeNsfw, int limit = 32, params FlufflePlatform[] platform);
    }

    public class ReverseSearchClient : IReverseSearchClient
    {
        private readonly IFlurlClient _client;

        public ReverseSearchClient(string applicationName)
        {
            _client = new FlurlClient("https://api.fluffle.xyz/v1")
                .WithHeader("User-Agent", Project.UserAgent(applicationName));
        }

        public async Task<FluffleResponse> ReverseSearchAsync(Func<Stream> openStream, bool includeNsfw, int limit = 32, params FlufflePlatform[] platforms)
        {
            var response = await _client.Request("search")
                .PostMultipartAsync(content =>
                {
                    foreach (var platform in platforms)
                        content.AddString("platforms", Enum.GetName(platform));

                    content.AddString("includeNsfw", includeNsfw.ToString());
                    content.AddString("limit", limit.ToString());
                    content.AddFile("file", openStream(), "file");
                });

            return await response.GetJsonAsync<FluffleResponse>();
        }
    }
}

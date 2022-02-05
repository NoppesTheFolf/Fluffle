using Flurl.Http;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Bot
{
    public class FluffleClient : ApiClient
    {
        public FluffleClient(string baseUrl = "https://api.fluffle.xyz") : base(baseUrl)
        {
            FlurlClient.Headers.Add("User-Agent", Project.UserAgent);
        }

        public async Task<FluffleResponse> SearchAsync(Stream stream, bool includeNsfw, int limit)
        {
            var response = await Request("v1", "search")
                .PostMultipartAsync(content =>
                {
                    content.AddFile("file", stream, "dummy");
                    content.AddString("includeNsfw", includeNsfw.ToString());
                    content.AddString("limit", limit.ToString());
                });

            return await response.GetJsonAsync<FluffleResponse>();
        }
    }

    public class FluffleStats
    {
        public int ElapsedMilliseconds { get; set; }

        public int Count { get; set; }
    }

    public enum FluffleMatch
    {
        Exact = 1,
        TossUp = 2,
        Alternative = 3,
        Unlikely = 4
    }

    public class FluffleResult
    {
        public int Id { get; set; }

        public float Score { get; set; }

        public FluffleMatch Match { get; set; }

        public FlufflePlatform Platform { get; set; }

        public string Location { get; set; }

        public bool IsSfw { get; set; }

        public class FluffleThumbnail
        {
            public int Width { get; set; }

            public int CenterX { get; set; }

            public int Height { get; set; }

            public int CenterY { get; set; }

            public string Location { get; set; }
        }

        public FluffleThumbnail Thumbnail { get; set; }

        public class FluffleCredit
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public ICollection<FluffleCredit> Credits { get; set; }
    }

    public class FluffleResponse
    {
        public FluffleStats Stats { get; set; }

        public IList<FluffleResult> Results { get; set; }
    }

    public enum FlufflePlatform
    {
        E621 = 1,
        FurryNetwork = 2,
        FurAffinity = 3,
        Weasyl = 4,
        Twitter = 5
    }

    public static class FlufflePlatformExtensions
    {
        public static string Pretty(this FlufflePlatform platform)
        {
            return platform switch
            {
                FlufflePlatform.E621 => "e621",
                FlufflePlatform.FurryNetwork => "Furry Network",
                FlufflePlatform.FurAffinity => "Fur Affinity",
                FlufflePlatform.Weasyl => "Weasyl",
                FlufflePlatform.Twitter => "Twitter",
                _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
            };
        }

        public static int Priority(this FlufflePlatform platform)
        {
            return platform switch
            {
                FlufflePlatform.E621 => 3,
                FlufflePlatform.FurryNetwork => 5,
                FlufflePlatform.FurAffinity => 1,
                FlufflePlatform.Weasyl => 4,
                FlufflePlatform.Twitter => 2,
                _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
            };
        }
    }
}

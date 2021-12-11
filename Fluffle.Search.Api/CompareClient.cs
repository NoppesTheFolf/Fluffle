using Flurl.Http;
using Newtonsoft.Json;
using Noppes.Fluffle.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api
{
    public class ComparedImage
    {
        [JsonProperty("id")]
        public int Id { get; }

        [JsonProperty("mismatch_count")]
        public ulong MismatchCount { get; }

        public ComparedImage(int id, ulong mismatchCount)
        {
            Id = id;
            MismatchCount = mismatchCount;
        }
    }

    public class CompareResult
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("images")]
        public ICollection<ComparedImage> Images { get; set; }
    }

    public interface ICompareClient
    {
        Task<IDictionary<int, CompareResult>> CompareAsync(ulong hash, bool includeNsfw, int limit);
    }

    public class CompareClient : ApiClient, ICompareClient
    {
        public CompareClient(string baseUrl) : base(baseUrl)
        {
        }

        public Task<IDictionary<int, CompareResult>> CompareAsync(ulong hash, bool includeNsfw, int limit)
        {
            return FlurlClient
                .Request("compare", hash, includeNsfw.ToString().ToLowerInvariant(), limit)
                .GetJsonAsync<IDictionary<int, CompareResult>>();
        }
    }
}

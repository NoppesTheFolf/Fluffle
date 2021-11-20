using Flurl.Http;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.PerceptualHashing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api
{
    public class CompareResult
    {
        public int Count { get; set; }

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

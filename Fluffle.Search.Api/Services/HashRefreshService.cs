using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.Search.Business.Similarity;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.Services;

public class HashRefreshService : IService
{
    private readonly ISimilarityService _similarityService;

    public HashRefreshService(ISimilarityService similarityService)
    {
        _similarityService = similarityService;
    }

    public async Task RunAsync()
    {
        await _similarityService.RefreshAsync();
    }
}

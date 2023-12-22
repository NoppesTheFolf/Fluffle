using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.Search.Business.Similarity;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.Services;

public class HashRefreshService : IService, IInitializable
{
    private bool _isFirstRun, _restoreSuccess;
    private readonly ISimilarityService _similarityService;

    public HashRefreshService(ISimilarityService similarityService)
    {
        _similarityService = similarityService;
    }

    public async Task InitializeAsync()
    {
        _isFirstRun = true;
        _restoreSuccess = await _similarityService.TryRestoreDumpAsync();
    }

    public async Task RunAsync()
    {
        await _similarityService.RefreshAsync();

        if (!_restoreSuccess || !_isFirstRun)
            await _similarityService.CreateDumpAsync();

        _isFirstRun = false;
    }
}

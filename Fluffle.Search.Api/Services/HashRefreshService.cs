using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.Search.Business.Similarity;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.Services;

public class HashRefreshService : IService, IInitializable
{
    private readonly TimeSpan _dumpInterval;

    private DateTime? _lastDumpWhen;
    private readonly ISimilarityService _similarityService;

    public HashRefreshService(ISimilarityService similarityService, TimeSpan dumpInterval)
    {
        _similarityService = similarityService;
        _dumpInterval = dumpInterval;
    }

    public async Task InitializeAsync()
    {
        var restoredDump = await _similarityService.TryRestoreDumpAsync();
        if (restoredDump == null)
            return;

        _lastDumpWhen = restoredDump.When;
    }

    public async Task RunAsync()
    {
        await _similarityService.RefreshAsync();

        var now = DateTime.UtcNow;
        var timeSinceLastDump = _lastDumpWhen == null
            ? (TimeSpan?)null
            : now.Subtract(_lastDumpWhen.Value);

        if (timeSinceLastDump == null || timeSinceLastDump.Value > _dumpInterval)
        {
            await _similarityService.CreateDumpAsync();
            _lastDumpWhen = now;
        }
    }
}

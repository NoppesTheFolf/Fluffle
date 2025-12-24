using Noppes.Fluffle.Bot.ReverseSearch.Api;
using Noppes.Fluffle.Utils;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Bot.ReverseSearch;

public class ReverseSearchScheduler : WorkScheduler<ReverseSearchSchedulerItem, int, FluffleApiResponse>
{
    private readonly FluffleApiClient _fluffleApiClient;

    public ReverseSearchScheduler(int numberOfWorkers, FluffleApiClient fluffleApiClient) : base(numberOfWorkers)
    {
        _fluffleApiClient = fluffleApiClient;
    }

    protected override async Task<FluffleApiResponse> HandleAsync(ReverseSearchSchedulerItem item)
    {
        return await _fluffleApiClient.ExactSearchAsync(item.Stream, item.Limit);
    }
}

using Microsoft.Extensions.Options;
using Noppes.Fluffle.Bot.ReverseSearch.Api;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Bot.ReverseSearch;

public class ReverseSearchScheduler : WorkScheduler<ReverseSearchSchedulerItem, int, FluffleApiResponse>
{
    private readonly FluffleApiClient _fluffleApiClient;

    public ReverseSearchScheduler(IOptions<BotConfiguration> options, FluffleApiClient fluffleApiClient) : base(options.Value.ReverseSearch.Workers)
    {
        _fluffleApiClient = fluffleApiClient;
    }

    protected override async Task<FluffleApiResponse> HandleAsync(ReverseSearchSchedulerItem item)
    {
        return await _fluffleApiClient.ExactSearchAsync(item.Stream, item.Limit);
    }
}

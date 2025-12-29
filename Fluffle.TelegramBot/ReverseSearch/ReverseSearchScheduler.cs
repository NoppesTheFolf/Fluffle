using System.Threading.Tasks;
using Fluffle.TelegramBot.ReverseSearch.Api;
using Microsoft.Extensions.Options;

namespace Fluffle.TelegramBot.ReverseSearch;

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

using System.IO;
using Fluffle.TelegramBot.ReverseSearch.Api;

namespace Fluffle.TelegramBot.ReverseSearch;

public class ReverseSearchSchedulerItem : WorkSchedulerItem<FluffleApiResponse>
{
    public Stream Stream { get; set; }

    public int Limit { get; set; }
}

using Noppes.Fluffle.Bot.ReverseSearch.Api;
using Noppes.Fluffle.Utils;
using System.IO;

namespace Noppes.Fluffle.Bot.ReverseSearch;

public class ReverseSearchSchedulerItem : WorkSchedulerItem<FluffleApiResponse>
{
    public Stream Stream { get; set; }

    public int Limit { get; set; }
}

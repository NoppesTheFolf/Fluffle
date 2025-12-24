using System.Collections.Generic;

namespace Noppes.Fluffle.Bot.ReverseSearch.Api;

public class FluffleApiResponse
{
    public required IList<FluffleApiResult> Results { get; set; }
}
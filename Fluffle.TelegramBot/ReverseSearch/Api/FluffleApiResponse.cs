using System.Collections.Generic;

namespace Fluffle.TelegramBot.ReverseSearch.Api;

public class FluffleApiResponse
{
    public required IList<FluffleApiResult> Results { get; set; }
}
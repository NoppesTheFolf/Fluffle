using System;
using System.Text.Json.Serialization;

namespace Fluffle.TelegramBot.ReverseSearch.Api;

public class FluffleApiResult
{
    public required FluffleApiMatch Match { get; set; }

    [JsonPropertyName("platform")]
    public required string PlatformName { get; set; }

    [JsonIgnore]
    public FluffleApiPlatform Platform => Enum.TryParse<FluffleApiPlatform>(PlatformName, ignoreCase: true, out var platform) ? platform : FluffleApiPlatform.Unknown;

    public required string Url { get; set; }
}

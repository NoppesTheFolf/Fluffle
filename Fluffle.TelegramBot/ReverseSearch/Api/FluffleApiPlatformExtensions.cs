using System;

namespace Fluffle.TelegramBot.ReverseSearch.Api;

public static class FluffleApiPlatformExtensions
{
    public static string Pretty(this FluffleApiPlatform platform)
    {
        return platform switch
        {
            FluffleApiPlatform.E621 => "e621",
            FluffleApiPlatform.FurryNetwork => "Furry Network",
            FluffleApiPlatform.FurAffinity => "Fur Affinity",
            FluffleApiPlatform.Weasyl => "Weasyl",
            FluffleApiPlatform.Twitter => "Twitter",
            FluffleApiPlatform.Bluesky => "Bluesky",
            _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
        };
    }

    public static int Priority(this FluffleApiPlatform platform)
    {
        return platform switch
        {
            FluffleApiPlatform.FurAffinity => 1,
            FluffleApiPlatform.Bluesky => 2,
            FluffleApiPlatform.Twitter => 3,
            FluffleApiPlatform.E621 => 4,
            FluffleApiPlatform.Weasyl => 5,
            FluffleApiPlatform.FurryNetwork => 6,
            _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
        };
    }

    public static int InlineKeyboardSize(this FluffleApiPlatform platform)
    {
        return platform switch
        {
            FluffleApiPlatform.FurAffinity => 79,
            FluffleApiPlatform.Bluesky => 54,
            FluffleApiPlatform.Twitter => 54,
            FluffleApiPlatform.E621 => 36,
            FluffleApiPlatform.Weasyl => 51,
            FluffleApiPlatform.FurryNetwork => 100,
            _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
        };
    }
}
using Humanizer;

namespace Noppes.Fluffle.Main.Api.Helpers;

public static class TagHelper
{
    public static string Normalize(string tag) => tag.ToLowerInvariant().Trim().Kebaberize();
}

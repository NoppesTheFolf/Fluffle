using System.Text.Json;

namespace Noppes.Fluffle.Api;

/// <summary>
/// Sometimes we need to manually serialize an object into JSON with the same settings as used
/// by ASP.NET Core. This class provides this functionality.
/// </summary>
public static class AspNetJsonSerializer
{
    internal static JsonSerializerOptions Options { private get; set; }

    /// <summary>
    /// Converts the given object into a JSON string.
    /// </summary>
    public static string Serialize(object value) => JsonSerializer.Serialize(value, Options);
}

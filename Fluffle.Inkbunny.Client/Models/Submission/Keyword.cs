using Newtonsoft.Json;

namespace Noppes.Fluffle.Inkbunny.Client.Models;

public class Keyword
{
    [JsonProperty("keyword_name")]
    public string Name { get; set; } = null!;
}

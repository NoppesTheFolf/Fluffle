using Newtonsoft.Json;

namespace Noppes.Fluffle.DeviantArt.Client.Models;

public class Tag
{
    [JsonProperty("tag_name")]
    public string Name { get; set; } = null!;
}
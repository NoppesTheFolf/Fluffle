using Newtonsoft.Json;

namespace Noppes.Fluffle.DeviantArt.Client.Models;

public class Error
{
    [JsonProperty("error")]
    public string Name { get; set; } = null!;

    [JsonProperty("error_description")]
    public string Description { get; set; } = null!;

    [JsonProperty("error_code")]
    public int? Code { get; set; }
}

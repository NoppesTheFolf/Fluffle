using Newtonsoft.Json;

namespace Noppes.Fluffle.DeviantArt.Client.Models;

public class User
{
    [JsonProperty("userid")]
    public string Id { get; set; } = null!;

    public string Username { get; set; } = null!;

    [JsonProperty("usericon")]
    public string IconLocation { get; set; } = null!;

    public UserDetails? Details { get; set; }
}


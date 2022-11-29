using Newtonsoft.Json;

namespace Noppes.Fluffle.DeviantArt.Client.Models;

public class UserDetails
{
    [JsonProperty("joindate")]
    public DateTimeOffset JoinedWhen { get; set; }
}

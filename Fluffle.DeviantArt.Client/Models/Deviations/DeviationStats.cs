using Newtonsoft.Json;

namespace Noppes.Fluffle.DeviantArt.Client.Models;

public class DeviationStats
{
    public int Comments { get; set; }

    [JsonProperty("favourites")]
    public int Favorites { get; set; }

    public int? Views { get; set; }
}

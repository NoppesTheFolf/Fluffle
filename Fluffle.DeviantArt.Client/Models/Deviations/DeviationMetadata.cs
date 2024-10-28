using Newtonsoft.Json;

namespace Noppes.Fluffle.DeviantArt.Client.Models;

public class DeviationMetadata
{
    [JsonProperty("deviationid")]
    public string Id { get; set; } = null!;

    public User Author { get; set; } = null!;

    public string Title { get; set; } = null!;

    public bool IsMature { get; set; }

    public ICollection<Tag> Tags { get; set; } = null!;

    public DeviationStats? Stats { get; set; } = null!;
}

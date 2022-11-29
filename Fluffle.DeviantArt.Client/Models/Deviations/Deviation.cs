using Newtonsoft.Json;

namespace Noppes.Fluffle.DeviantArt.Client.Models;

public class Deviation
{
    [JsonProperty("deviationid")]
    public string Id { get; set; } = null!;

    public User Author { get; set; } = null!;

    public string Url { get; set; } = null!;

    public string Title { get; set; } = null!;

    public bool IsMature { get; set; }

    public long PublishedTime { get; set; }

    public DateTimeOffset PublishedWhen => DateTimeOffset.FromUnixTimeSeconds(PublishedTime);

    public DeviationStats? Stats { get; set; }

    public DeviationImageFile? Preview { get; set; }

    public DeviationImageFile? Content { get; set; }

    [JsonProperty("thumbs")]
    public ICollection<DeviationImageFile>? Thumbnails { get; set; }

    public ICollection<DeviationVideoFile>? Videos { get; set; }

    public DeviationFlashFile? Flash { get; set; }

    public DeviationTier? Tier { get; set; }
}

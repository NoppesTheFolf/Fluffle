using Newtonsoft.Json;

namespace Noppes.Fluffle.Inkbunny.Client.Models;

public class Submission
{
    [JsonProperty("submission_id")]
    public string Id { get; set; } = null!;

    public ICollection<Keyword> Keywords { get; set; } = null!;

    public ICollection<SubmissionFile> Files { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    [JsonProperty("rating_id")]
    public SubmissionRating Rating { get; set; }

    public int Views { get; set; }

    [JsonProperty("create_datetime")]
    public DateTimeOffset CreatedWhen { get; set; }
}

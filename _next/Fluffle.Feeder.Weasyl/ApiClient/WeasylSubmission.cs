using System.Text.Json.Serialization;

namespace Fluffle.Feeder.Weasyl.ApiClient;

internal class WeasylSubmission
{
    [JsonPropertyName("submitid")]
    public required int SubmitId { get; set; }

    public required string Owner { get; set; }

    public required string OwnerLogin { get; set; }

    public required DateTimeOffset PostedAt { get; set; }

    public required WeasylSubmissionMediaKeys Media { get; set; }

    public required WeasylSubmissionSubtype Subtype { get; set; }

    public required WeasylSubmissionRating Rating { get; set; }
}

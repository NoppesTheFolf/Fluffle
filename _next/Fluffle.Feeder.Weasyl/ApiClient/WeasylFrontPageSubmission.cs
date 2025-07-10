using System.Text.Json.Serialization;

namespace Fluffle.Feeder.Weasyl.ApiClient;

internal class WeasylFrontPageSubmission
{
    [JsonPropertyName("submitid")]
    public int? SubmitId { get; set; } // Character submissions don't have a submission ID
}

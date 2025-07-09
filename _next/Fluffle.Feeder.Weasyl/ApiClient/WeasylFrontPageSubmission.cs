using System.Text.Json.Serialization;

namespace Fluffle.Feeder.Weasyl.ApiClient;

internal class WeasylFrontPageSubmission
{
    [JsonPropertyName("submitid")]
    public required int SubmitId { get; set; }
}

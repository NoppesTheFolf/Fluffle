using Fluffle.Feeder.Inkbunny.Client.Converters;
using System.Text.Json.Serialization;

namespace Fluffle.Feeder.Inkbunny.Client.Models;

internal class InkbunnySubmission
{
    [JsonPropertyName("submission_id")]
    public required string Id { get; set; }

    public required IList<InkbunnySubmissionFile> Files { get; set; }

    public required string Username { get; set; }

    public required string UserId { get; set; }

    [JsonPropertyName("rating_id"), JsonConverter(typeof(InkbunnyRatingConverter))]
    public InkbunnySubmissionRating Rating { get; set; }

    [JsonPropertyName("create_datetime"), JsonConverter(typeof(InkbunnyDateTimeOffsetConverter))]
    public DateTimeOffset CreatedWhen { get; set; }
}

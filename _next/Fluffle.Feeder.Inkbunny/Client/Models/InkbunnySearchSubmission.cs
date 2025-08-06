using Fluffle.Feeder.Inkbunny.Client.Converters;
using System.Text.Json.Serialization;

namespace Fluffle.Feeder.Inkbunny.Client.Models;

internal class InkbunnySearchSubmission
{
    [JsonPropertyName("submission_id")]
    public required string Id { get; set; }

    [JsonPropertyName("create_datetime"), JsonConverter(typeof(InkbunnyDateTimeOffsetConverter))]
    public DateTimeOffset CreatedWhen { get; set; }
}

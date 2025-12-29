using System.Text.Json.Serialization;

namespace Fluffle.Feeder.Inkbunny.Client.Models;

internal class InkbunnySubmissionFile
{
    [JsonPropertyName("file_id")]
    public required string Id { get; set; }

    [JsonPropertyName("file_url_full")]
    public required string FullFileUrl { get; set; }

    [JsonPropertyName("full_size_x")]
    public int? FullFileWidth { get; set; }

    [JsonPropertyName("full_size_y")]
    public int? FullFileHeight { get; set; }

    [JsonPropertyName("thumbnail_url_medium_noncustom")]
    public string? NonCustomMediumThumbnailUrl { get; set; }

    [JsonPropertyName("thumb_medium_noncustom_x")]
    public int? NonCustomMediumThumbnailWidth { get; set; }

    [JsonPropertyName("thumb_medium_noncustom_y")]
    public int? NonCustomMediumThumbnailHeight { get; set; }

    [JsonPropertyName("thumbnail_url_large_noncustom")]
    public string? NonCustomLargeThumbnailUrl { get; set; }

    [JsonPropertyName("thumb_large_noncustom_x")]
    public int? NonCustomLargeThumbnailWidth { get; set; }

    [JsonPropertyName("thumb_large_noncustom_y")]
    public int? NonCustomLargeThumbnailHeight { get; set; }

    [JsonPropertyName("thumbnail_url_huge_noncustom")]
    public string? NonCustomHugeThumbnailUrl { get; set; }

    [JsonPropertyName("thumb_huge_noncustom_x")]
    public int? NonCustomHugeThumbnailWidth { get; set; }

    [JsonPropertyName("thumb_huge_noncustom_y")]
    public int? NonCustomHugeThumbnailHeight { get; set; }
}

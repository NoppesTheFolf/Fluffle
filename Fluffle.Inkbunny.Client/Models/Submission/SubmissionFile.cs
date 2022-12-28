using Newtonsoft.Json;

namespace Noppes.Fluffle.Inkbunny.Client.Models;

public class SubmissionFile
{
    [JsonProperty("file_id")]
    public string Id { get; set; } = null!;

    public string SubmissionId { get; set; } = null!;

    [JsonProperty("submission_file_order")]
    public double Order { get; set; }

    [JsonProperty("file_url_full")]
    public string? FullFileUrl { get; set; }

    [JsonProperty("full_size_x")]
    public int? FullFileWidth { get; set; }

    [JsonProperty("full_size_y")]
    public int? FullFileHeight { get; set; }

    [JsonProperty("file_url_screen")]
    public string? ScreenFileUrl { get; set; }

    [JsonProperty("screen_size_x")]
    public int? ScreenFileWidth { get; set; }

    [JsonProperty("screen_size_y")]
    public int? ScreenFileHeight { get; set; }

    [JsonProperty("file_url_preview")]
    public string? PreviewFileUrl { get; set; }

    [JsonProperty("preview_size_x")]
    public int? PreviewFileWidth { get; set; }

    [JsonProperty("preview_size_y")]
    public int? PreviewFileHeight { get; set; }

    [JsonProperty("thumbnail_url_medium_noncustom")]
    public string? NonCustomMediumThumbnailUrl { get; set; }

    [JsonProperty("thumb_medium_noncustom_x")]
    public int? NonCustomMediumThumbnailWidth { get; set; }

    [JsonProperty("thumb_medium_noncustom_y")]
    public int? NonCustomMediumThumbnailHeight { get; set; }

    [JsonProperty("thumbnail_url_large_noncustom")]
    public string? NonCustomLargeThumbnailUrl { get; set; }

    [JsonProperty("thumb_large_noncustom_x")]
    public int? NonCustomLargeThumbnailWidth { get; set; }

    [JsonProperty("thumb_large_noncustom_y")]
    public int? NonCustomLargeThumbnailHeight { get; set; }

    [JsonProperty("thumbnail_url_huge_noncustom")]
    public string? NonCustomHugeThumbnailUrl { get; set; }

    [JsonProperty("thumb_huge_noncustom_x")]
    public int? NonCustomHugeThumbnailWidth { get; set; }

    [JsonProperty("thumb_huge_noncustom_y")]
    public int? NonCustomHugeThumbnailHeight { get; set; }

    [JsonProperty("mimetype")]
    public string MimeType { get; set; } = null!;
}

namespace Noppes.Fluffle.B2;

/// <summary>
/// Represents an object in a bucket.
/// </summary>
public class B2File
{
    public string AccountId { get; set; }

    public string Action { get; set; }

    public string BucketId { get; set; }

    public int ContentLength { get; set; }

    public string ContentSha1 { get; set; }

    public string ContentType { get; set; }

    public string FileId { get; set; }

    public B2FileInfo FileInfo { get; set; }

    public string FileName { get; set; }

    public object UploadTimestamp { get; set; }
}

namespace Noppes.Fluffle.B2
{
    /// <summary>
    /// The response received after uploading an object to B2. Provides an additional field, <see
    /// cref="DownloadUrl"/>, which is the URL at which the uploaded object is available.
    /// </summary>
    public class B2UploadResponse
    {
        public string AccountId { get; set; }

        public B2Action Action { get; set; }

        public string BucketId { get; set; }

        public int ContentLength { get; set; }

        public string ContentMd5 { get; set; }

        public string ContentSha1 { get; set; }

        public string ContentType { get; set; }

        public string FileId { get; set; }

        public string FileName { get; set; }

        public long UploadTimestamp { get; set; }

        public string DownloadUrl { get; set; }
    }
}

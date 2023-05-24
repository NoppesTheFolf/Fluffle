namespace Noppes.Fluffle.B2;

/// <summary>
/// The response received when a request is made to get information about where objects can be uploaded.
/// </summary>
public class B2UploadInformation
{
    public string BucketId { get; set; }

    public string UploadUrl { get; set; }

    public string AuthorizationToken { get; set; }
}

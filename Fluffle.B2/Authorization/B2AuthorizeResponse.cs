using System.Collections.Generic;

namespace Noppes.Fluffle.B2;

/// <summary>
/// The response received from the B2 Api when authorizing using an application key.
/// </summary>
public class B2AuthorizeResponse
{
    public string AccountId { get; set; }

    public class B2AuthorizedBucket
    {
        public string BucketId { get; set; }

        public string BucketName { get; set; }

        public ICollection<B2KeyCapability> Capabilities { get; set; }

        public string NamePrefix { get; set; }
    }

    public B2AuthorizedBucket Allowed { get; set; }

    public string ApiUrl { get; set; }

    public string AuthorizationToken { get; set; }

    public int AbsoluteMinimumPartSize { get; set; }

    public int RecommendedPartSize { get; set; }

    public string DownloadUrl { get; set; }
}

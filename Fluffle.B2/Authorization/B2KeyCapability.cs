namespace Noppes.Fluffle.B2;

/// <summary>
/// The permissions (called capabilities by Backblaze) which can be tied to an API key.
/// </summary>
public enum B2KeyCapability
{
    ListKeys,
    WriteKeys,
    DeleteKeys,
    ListBuckets,
    ReadBuckets,
    WriteBuckets,
    DeleteBuckets,
    ListFiles,
    ReadFiles,
    ShareFiles,
    WriteFiles,
    DeleteFiles,
    ReadBucketEncryption,
    WriteBucketEncryption,
    WriteBucketReplications,
    ReadBucketReplications
}

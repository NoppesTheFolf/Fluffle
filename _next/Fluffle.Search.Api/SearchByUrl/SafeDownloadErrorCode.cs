namespace Fluffle.Search.Api.SearchByUrl;

public enum SafeDownloadErrorCode
{
    Unparsable,
    InvalidScheme,
    HostNotFound,
    NoIpAddresses,
    NoPublicIpAddresses,
    NonSuccessStatusCode,
    FileTooBig
}

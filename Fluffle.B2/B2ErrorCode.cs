namespace Noppes.Fluffle.B2;

/// <summary>
/// Error codes occurring in error responses by the B2 API (<see cref="B2ErrorResponse.Code"/>).
/// </summary>
public static class B2ErrorCode
{
    public static readonly string BadAuthorizationToken = "bad_auth_token";

    public static readonly string ExpiredAuthorizationToken = "expired_auth_token";

    public static readonly string FileNotPresent = "file_not_present";
}

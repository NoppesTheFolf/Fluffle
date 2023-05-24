namespace Noppes.Fluffle.Api.AccessControl;

/// <summary>
/// Marks a class as one which contains permissions.
/// </summary>
public abstract class Permissions
{
    /// <summary>
    /// The prefix used for permission claims.
    /// </summary>
    public const string ClaimPrefix = "PERMISSION_";
}

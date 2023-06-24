namespace Noppes.Fluffle.Twitter.Database;

public class UserImportFailureEntity
{
    /// <summary>
    /// Username of the user for which the import failed.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Why the import failed.
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// When that 
    /// </summary>
    public DateTime ImportedAt { get; set; }
}

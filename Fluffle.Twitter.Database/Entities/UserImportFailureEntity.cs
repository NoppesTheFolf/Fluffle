using MongoDB.Bson;

namespace Noppes.Fluffle.Twitter.Database;

public class UserImportFailureEntity
{
    /// <summary>
    /// Unique identifier for this record.
    /// </summary>
    public ObjectId Id { get; set; }

    /// <summary>
    /// Username of the user for which the import failed.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Why the import failed.
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// When that 
    /// </summary>
    public DateTime ImportedAt { get; set; }
}

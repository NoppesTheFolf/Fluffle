namespace Noppes.Fluffle.DeviantArt.Client.Models;

public abstract class PaginatedResponse<T>
{
    public bool HasMore { get; set; }

    public int? NextOffset { get; set; }

    public ICollection<T> Results { get; set; } = null!;
}

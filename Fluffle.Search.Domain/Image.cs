namespace Noppes.Fluffle.Search.Domain;

public class Image
{
    public int Id { get; set; }

    public bool IsSfw { get; set; }

    public long ChangeId { get; set; }

    public bool IsDeleted { get; set; }

    public ulong PhashAverage64 { get; set; }

    public ulong[] PhashAverage256 { get; set; } = null!;
}

namespace Noppes.Fluffle.Search.Database;

public interface ITrackable
{
    public long ChangeId { get; set; }
    public int PlatformId { get; set; }
}

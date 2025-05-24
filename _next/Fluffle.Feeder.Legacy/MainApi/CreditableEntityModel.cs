namespace Fluffle.Feeder.Legacy.MainApi;

public class CreditableEntityModel
{
    public int Id { get; set; }

    public int PlatformId { get; set; }

    public long ChangeId { get; set; }

    public string Name { get; set; } = null!;

    public string IdOnPlatform { get; set; } = null!;

    public int? Priority { get; set; }
}

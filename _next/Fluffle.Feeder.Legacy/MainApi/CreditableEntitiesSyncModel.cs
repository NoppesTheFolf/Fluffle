namespace Fluffle.Feeder.Legacy.MainApi;

public class CreditableEntitiesSyncModel
{
    public long NextChangeId { get; set; }

    public ICollection<CreditableEntityModel> Results { get; set; } = null!;
}

namespace Fluffle.Feeder.E621;

public class E621FeederState
{
    public required DateTime LastRunWhen { get; set; }

    public required int? CurrentId { get; set; }
}

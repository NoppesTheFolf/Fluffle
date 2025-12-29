namespace Fluffle.Feeder.E621;

internal class E621FeederState
{
    public required DateTime LastRunWhen { get; set; }

    public required int? CurrentId { get; set; }
}

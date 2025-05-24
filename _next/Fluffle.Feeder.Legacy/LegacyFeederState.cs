namespace Fluffle.Feeder.Legacy;

public class LegacyFeederState
{
    public required DateTime? LastRunWhen { get; set; }

    public required IDictionary<string, long> Platforms { get; set; }
}

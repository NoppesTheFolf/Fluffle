namespace Fluffle.Feeder.Inkbunny;

internal class InkbunnyFeederState
{
    public required DateTime LastRunWhen { get; set; }

    public required int CurrentId { get; set; }

    public required DateTime RetrieveUntil { get; set; }
}

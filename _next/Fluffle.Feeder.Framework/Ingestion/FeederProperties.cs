namespace Fluffle.Feeder.Framework.Ingestion;

public class FeederProperties
{
    public required string Url { get; set; }

    public required bool? IsSfw { get; set; }

    public required ICollection<FeederAuthor> Authors { get; set; }

    public required DateTime CreatedWhen { get; set; }
}

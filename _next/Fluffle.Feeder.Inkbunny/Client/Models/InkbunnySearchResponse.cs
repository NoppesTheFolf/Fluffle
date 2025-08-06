namespace Fluffle.Feeder.Inkbunny.Client.Models;

internal class InkbunnySearchResponse
{
    public required ICollection<InkbunnySearchSubmission> Submissions { get; set; }
}

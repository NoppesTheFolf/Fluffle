namespace Fluffle.Feeder.Weasyl;

internal class WeasylFeederState
{
    public required int StartSubmissionId { get; set; }

    public required int EndSubmissionId { get; set; }

    public required int CurrentSubmissionId { get; set; }
}

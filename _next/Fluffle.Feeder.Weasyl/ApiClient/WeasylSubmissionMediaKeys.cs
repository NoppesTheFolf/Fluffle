namespace Fluffle.Feeder.Weasyl.ApiClient;

internal class WeasylSubmissionMediaKeys
{
    public ICollection<WeasylSubmissionMedia>? Cover { get; set; }

    public required ICollection<WeasylSubmissionMedia> Submission { get; set; }
}

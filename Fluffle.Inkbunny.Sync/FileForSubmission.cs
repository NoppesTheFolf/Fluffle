using Noppes.Fluffle.Inkbunny.Client.Models;

namespace Noppes.Fluffle.Inkbunny.Sync;

public class FileForSubmission
{
    public Submission Submission { get; }

    public SubmissionFile File { get; }

    public int Page { get; }

    public FileForSubmission(Submission submission, SubmissionFile file, int page)
    {
        Submission = submission;
        File = file;
        Page = page;
    }
}

using Noppes.Fluffle.Inkbunny.Client.Models;

namespace Noppes.Fluffle.Inkbunny.Sync;

public class FileForSubmission
{
    public Submission Submission { get; }

    public SubmissionFile File { get; }

    public FileForSubmission(Submission submission, SubmissionFile file)
    {
        Submission = submission;
        File = file;
    }
}

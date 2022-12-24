using Noppes.Fluffle.Inkbunny.Client.Models;

namespace Noppes.Fluffle.Inkbunny.Client;

public interface IInkbunnyClient
{
    Task<SubmissionsResponse> SearchSubmissionsAsync(SubmissionSearchOrder order);

    Task<SubmissionsResponse> GetSubmissionsAsync(IEnumerable<string> ids);
}

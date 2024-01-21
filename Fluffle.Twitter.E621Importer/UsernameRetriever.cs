using System.Text.RegularExpressions;

namespace Noppes.Fluffle.Twitter.E621Importer;

internal class UsernameRetriever
{
    /// <summary>
    /// Matches URLs that contain a semantically valid Twitter handle.
    /// </summary>
    private static readonly Regex UsernameRegex = new("(?<=^|\\/\\/)(?:twitter|x)\\.com\\/([A-Za-z0-9_]{1,15})(?=\\/|$|\\?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly PostSourceRetriever _postSourceRetriever;
    private readonly ArtistsSourceRetriever _artistsSourceRetriever;

    public UsernameRetriever(PostSourceRetriever postSourceRetriever, ArtistsSourceRetriever artistsSourceRetriever)
    {
        _postSourceRetriever = postSourceRetriever;
        _artistsSourceRetriever = artistsSourceRetriever;
    }

    public async IAsyncEnumerable<(string username, Source source)> GetUsernamesAsync()
    {
        var usernames = new HashSet<string>();

        await foreach (var username in GetUsernamesAsync(usernames, _postSourceRetriever))
            yield return (username, Source.E621Post);

        await foreach (var username in GetUsernamesAsync(usernames, _artistsSourceRetriever))
            yield return (username, Source.E621Artist);
    }

    private static async IAsyncEnumerable<string> GetUsernamesAsync(ISet<string> usernames, ISourceRetriever retriever)
    {
        await foreach (var source in retriever.GetSourcesAsync())
        {
            var usernameMatch = UsernameRegex.Match(source);
            if (!usernameMatch.Success)
                continue;

            var username = usernameMatch.Groups[1].Value.ToLowerInvariant();
            var usernameAdded = usernames.Add(username);
            if (!usernameAdded)
                continue;

            yield return username;
        }
    }
}

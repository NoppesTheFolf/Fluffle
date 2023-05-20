using Noppes.E621;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.E621Sync;

namespace Noppes.Fluffle.Twitter.E621Importer;

internal class ArtistsSourceRetriever : ISourceRetriever
{
    private readonly IE621Client _e621Client;

    public ArtistsSourceRetriever(IE621Client e621Client)
    {
        _e621Client = e621Client;
    }

    public async IAsyncEnumerable<string> GetSourcesAsync()
    {
        await foreach (var artists in EnumerateArtistsAsync(0))
        {
            var urls = artists.SelectMany(x => x.Urls).Select(x => x.Url);
            foreach (var url in urls)
                yield return url;
        }
    }

    private async IAsyncEnumerable<ICollection<Artist>> EnumerateArtistsAsync(int startId)
    {
        var currentId = startId;
        while (true)
        {
            var artists = await LogEx.TimeAsync(async () =>
            {
                return await E621HttpResiliency.RunAsync(() =>
                    _e621Client.GetArtistsAsync(currentId, Position.After, limit: E621Constants.ArtistsMaximumLimit));
            }, "Retrieving artists after ID {afterId}", currentId);

            if (!artists.Any())
                break;

            var maxId = artists.Max(p => p.Id);
            yield return artists;

            if (artists.Count != E621Constants.ArtistsMaximumLimit)
                break;

            currentId = maxId;
        }
    }
}

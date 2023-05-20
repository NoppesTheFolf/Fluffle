using Noppes.E621;
using Noppes.E621.DbExport;

namespace Noppes.Fluffle.Twitter.E621Importer;

internal class PostSourceRetriever : ISourceRetriever
{
    private readonly IE621Client _e621Client;

    public PostSourceRetriever(IE621Client e621Client)
    {
        _e621Client = e621Client;
    }

    public async IAsyncEnumerable<string> GetSourcesAsync()
    {
        var dbExportClient = _e621Client.GetDbExportClient();
        var dbExports = await dbExportClient.GetDbExportsAsync();
        var postDbExport = dbExports.Latest(DbExportType.Post);

        await using var dbExportStream = await dbExportClient.GetDbExportStreamAsync(postDbExport);
        await foreach (var post in dbExportClient.ReadStreamAsPostsDbExportAsync(dbExportStream))
        {
            foreach (var source in post.Sources)
            {
                yield return source;
            }
        }
    }
}

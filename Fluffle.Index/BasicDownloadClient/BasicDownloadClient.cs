using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index;

public class BasicDownloadClient : DownloadClient
{
    private readonly BasicHttpClient _client;

    public BasicDownloadClient(BasicHttpClient client)
    {
        _client = client;
    }

    public override async Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken = default)
    {
        return await _client.GetStreamAsync(url);
    }
}

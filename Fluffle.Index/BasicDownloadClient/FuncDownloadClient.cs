using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index;

public class FuncDownloadClient : DownloadClient
{
    private readonly Func<string, Task<Stream>> _getStreamAsync;

    public FuncDownloadClient(Func<string, Task<Stream>> getStreamAsync)
    {
        _getStreamAsync = getStreamAsync;
    }

    public override async Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken = default)
    {
        return await _getStreamAsync(url);
    }
}

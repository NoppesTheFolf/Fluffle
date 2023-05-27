using Nitranium.PerceptualHashing.Utils;
using Noppes.E621;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.FurryNetworkSync;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Weasyl;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index;

public class WeasylDownloadClient : DownloadClient
{
    private readonly WeasylClient _client;

    public WeasylDownloadClient(WeasylClient client)
    {
        _client = client;
    }

    public override Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken = default) =>
        _client.GetStreamAsync(url);
}

public class FurryNetworkDownloadClient : DownloadClient
{
    private readonly FurryNetworkClient _client;

    public FurryNetworkDownloadClient(FurryNetworkClient client)
    {
        _client = client;
    }

    public override Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken = default) =>
        _client.GetStreamAsync(url);
}

public class FurAffinityDownloadClient : DownloadClient
{
    private readonly FurAffinityClient _faClient;

    public FurAffinityDownloadClient(FurAffinityClient faClient)
    {
        _faClient = faClient;
    }

    public override async Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken = default)
    {
        return await _faClient.GetStreamAsync(url);
    }
}

public class E621DownloadClient : DownloadClient
{
    private readonly IE621Client _client;

    public E621DownloadClient(IE621Client client)
    {
        _client = client;
    }

    public override Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken = default) =>
        _client.GetStreamAsync(url);
}

public abstract class DownloadClient
{
    public async Task<TemporaryFile> DownloadAsync(string url, CancellationToken cancellationToken = default)
    {
        var temporaryFile = new TemporaryFile();
        var temporaryFileStream = temporaryFile.OpenFileStream();

        try
        {
            await using var httpStream =
                await HttpResiliency.RunAsync(() => GetStreamAsync(url, cancellationToken));

            await httpStream.CopyToAsync(temporaryFileStream, cancellationToken);
        }
        catch
        {
            // We have to close the stream before the temporary object itself can be disposed.
            // If we don't do this, then the temporary file instance can't delete the file
            await temporaryFileStream.DisposeAsync();
            temporaryFile.Dispose();
            throw;
        }
        finally
        {
            // The file has been written to, we can get rid of the used stream
            await temporaryFileStream.DisposeAsync();
        }

        return temporaryFile;
    }

    public abstract Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken = default);
}

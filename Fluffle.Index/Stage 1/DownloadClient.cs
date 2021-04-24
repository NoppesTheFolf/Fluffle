using Humanizer;
using Nitranium.PerceptualHashing.Utils;
using Noppes.E621;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.FurryNetworkSync;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Serilog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index
{
    public class FurryNetworkDownloadClient : DownloadClient
    {
        private readonly FurryNetworkClient _client;

        public FurryNetworkDownloadClient(FurryNetworkClient client) : base(3.Seconds())
        {
            _client = client;
        }

        public override Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken = default) =>
            _client.GetStreamAsync(url);
    }

    public class FurAffinityDownloadClient : DownloadClient
    {
        private static readonly TimeSpan CheckInternal = 5.Minutes();

        private readonly FurAffinityClient _faClient;
        private readonly FluffleClient _fluffleClient;

        private long _newCheckAt;
        private bool _botsAllowed;

        public FurAffinityDownloadClient(FurAffinityClient faClient, FluffleClient fluffleClient) : base(1.Seconds())
        {
            _faClient = faClient;
            _fluffleClient = fluffleClient;
            _newCheckAt = -1;
            _botsAllowed = false;
        }

        public override async Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken = default)
        {
            do
            {
                var now = DateTimeOffset.UtcNow;
                if (_newCheckAt == -1 || now.ToUnixTimeSeconds() >= _newCheckAt)
                {
                    Log.Information("[{platform}] Checking if bots allowed...", "Fur Affinity");
                    _botsAllowed = await HttpResiliency.RunAsync(_fluffleClient.GetFaBotsAllowedAsync);
                    _newCheckAt = now.Add(CheckInternal).ToUnixTimeSeconds();
                }

                if (_botsAllowed)
                    continue;

                Log.Information("[{platform}] Bots not allowed. Waiting for {time} before checking again",
                    "Fur Affinity", CheckInternal.Humanize());
                await Task.Delay(CheckInternal, cancellationToken);
            } while (!_botsAllowed);

            return await _faClient.GetStreamAsync(url);
        }
    }

    public class E621DownloadClient : DownloadClient
    {
        private readonly IE621Client _client;

        public E621DownloadClient(IE621Client client) : base(null)
        {
            _client = client;
        }

        public override Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken = default) =>
            _client.GetStreamAsync(url);
    }

    public abstract class DownloadClient
    {
        private readonly RequestRateLimiter _rateLimiter;

        protected DownloadClient(TimeSpan? interval)
        {
            _rateLimiter = interval == null ? null : new RequestRateLimiter((TimeSpan)interval);
        }

        public async Task<TemporaryFile> DownloadAsync(string url, CancellationToken cancellationToken = default)
        {
            if (_rateLimiter != null)
                await _rateLimiter.InterceptAsync(null);

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
}

using Nitranium.PerceptualHashing.Utils;
using Noppes.Fluffle.Http;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index
{
    public class DownloadClient
    {
        private readonly RequestRateLimiter _rateLimiter;
        private readonly Func<string, CancellationToken, Task<Stream>> _getStreamAsync;

        public DownloadClient(TimeSpan? interval, Func<string, CancellationToken, Task<Stream>> getStreamAsync)
        {
            _rateLimiter = interval == null ? null : new RequestRateLimiter((TimeSpan)interval);
            _getStreamAsync = getStreamAsync;
        }

        public async Task<TemporaryFile> DownloadAsync(string url, CancellationToken cancellationToken = default)
        {
            if (_rateLimiter != null)
                await _rateLimiter.InterceptAsync(null);

            var temporaryFile = new TemporaryFile();
            var temporaryFileStream = temporaryFile.OpenFileStream();

            try
            {
                await using var httpStream = await HttpResiliency.RunAsync(() =>
                    _getStreamAsync(url, cancellationToken));

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
    }
}

using Nitranium.PerceptualHashing.Utils;
using Noppes.Fluffle.Http;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index
{
    public abstract class DownloadClient
    {
        protected abstract Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken = default);

        public async Task<TemporaryFile> DownloadAsync(string url, CancellationToken cancellationToken = default)
        {
            var temporaryFile = new TemporaryFile();
            var temporaryFileStream = temporaryFile.OpenFileStream();

            try
            {
                await using var httpStream = await HttpResiliency.RunAsync(() =>
                    GetStreamAsync(url, cancellationToken));

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

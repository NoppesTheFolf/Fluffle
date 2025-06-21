using FluentFTP;
using FluentFTP.Exceptions;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Fluffle.Content.Api.Storage;

public sealed class FtpStorage
{
    private static readonly Regex ValidPathRegex = new(@"^(?:[A-Za-z0-9]+\/)+[A-Za-z0-9]+\.[A-Za-z]+$",
        RegexOptions.NonBacktracking | RegexOptions.Compiled);

    private readonly FtpClientPool _ftpClientPool;
    private readonly IOptions<FtpStorageOptions> _options;
    private readonly ILogger<FtpStorage> _logger;

    public FtpStorage(FtpClientPool ftpClientPool, IOptions<FtpStorageOptions> options, ILogger<FtpStorage> logger)
    {
        _ftpClientPool = ftpClientPool;
        _options = options;
        _logger = logger;
    }

    public async Task PutAsync(string path, Stream stream)
    {
        if (!stream.CanSeek)
        {
            throw new ArgumentException("Stream must be seekable.", nameof(stream));
        }

        var ftpPath = Path.Join(_options.Value.DirectoryPrefix, path);
        var streamStartingPosition = stream.Position;
        await UseAsync(async ftpClient =>
        {
            try
            {
                await ftpClient.UploadStream(stream, ftpPath, FtpRemoteExists.Overwrite, true);

                var checksum = await ftpClient.GetChecksum(ftpPath, FtpHashAlgorithm.SHA1);

                stream.Position = streamStartingPosition;
                if (!checksum.Verify(stream))
                {
                    throw new InvalidOperationException("FTP SHA1 checksum check failed after upload.");
                }
            }
            catch
            {
                if (await ftpClient.FileExists(ftpPath))
                {
                    await ftpClient.DeleteFile(ftpPath);
                }

                throw;
            }
            finally
            {
                // Always reset the stream back to the starting position in case of a retry
                stream.Position = streamStartingPosition;
            }

            return true;
        });
    }

    public async Task<Stream?> GetAsync(string path)
    {
        if (!ValidPathRegex.IsMatch(path))
            return null;

        var ftpPath = Path.Join(_options.Value.DirectoryPrefix, path);
        return await UseAsync(async ftpClient =>
        {
            var stream = new MemoryStream();
            try
            {
                if (!await ftpClient.DownloadStream(stream, ftpPath))
                {
                    return null;
                }
            }
            catch (FtpMissingObjectException)
            {
                await stream.DisposeAsync();
                return null;
            }
            catch
            {
                await stream.DisposeAsync();
                throw;
            }

            stream.Position = 0;
            return stream;
        });
    }

    public async Task DeleteAsync(string path)
    {
        await UseAsync(async ftpClient =>
        {
            var ftpPath = Path.Join(_options.Value.DirectoryPrefix, path);
            if (await ftpClient.FileExists(ftpPath))
            {
                await ftpClient.DeleteFile(ftpPath);
            }

            return true;
        });
    }

    private async Task<T> UseAsync<T>(Func<AsyncFtpClient, Task<T>> useAsync)
    {
        var ftpClient = await _ftpClientPool.RentAsync();
        try
        {
            var result = await useAsync(ftpClient);
            _ftpClientPool.Return(ftpClient);
            return result;
        }
        catch (Exception e1)
        {
            if (e1 is FtpException)
            {
                _logger.LogWarning(e1, "A FTP exception occurred, retrying with a new client.");
                var newFtpClient = await _ftpClientPool.ReplaceAsync(ftpClient);
                try
                {
                    var result = await useAsync(newFtpClient);
                    _ftpClientPool.Return(newFtpClient);
                    return result;
                }
                catch
                {
                    _ftpClientPool.Return(newFtpClient);
                    throw;
                }
            }

            _ftpClientPool.Return(ftpClient);
            throw;
        }
    }
}

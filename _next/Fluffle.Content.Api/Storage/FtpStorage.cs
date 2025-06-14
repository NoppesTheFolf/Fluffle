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

    public FtpStorage(FtpClientPool ftpClientPool, IOptions<FtpStorageOptions> options)
    {
        _ftpClientPool = ftpClientPool;
        _options = options;
    }

    public async Task PutAsync(string path, Stream stream)
    {
        if (!stream.CanSeek)
        {
            throw new ArgumentException("Stream must be seekable.", nameof(stream));
        }

        var ftpPath = Path.Join(_options.Value.DirectoryPrefix, path);
        var ftpClient = await _ftpClientPool.RentAsync();
        try
        {
            var streamStartingPosition = stream.Position;
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
            _ftpClientPool.Return(ftpClient);
        }
    }

    public async Task<Stream?> GetAsync(string path)
    {
        if (!ValidPathRegex.IsMatch(path))
            return null;

        var ftpPath = Path.Join(_options.Value.DirectoryPrefix, path);
        var ftpClient = await _ftpClientPool.RentAsync();
        try
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

            stream.Position = 0;
            return stream;
        }
        finally
        {
            _ftpClientPool.Return(ftpClient);
        }
    }

    public async Task DeleteAsync(string path)
    {
        var ftpClient = await _ftpClientPool.RentAsync();
        try
        {
            var ftpPath = Path.Join(_options.Value.DirectoryPrefix, path);
            if (await ftpClient.FileExists(ftpPath))
            {
                await ftpClient.DeleteFile(ftpPath);
            }
        }
        finally
        {
            _ftpClientPool.Return(ftpClient);
        }
    }
}

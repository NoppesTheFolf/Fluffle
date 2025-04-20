using FluentFTP;
using Microsoft.Extensions.Options;
using Nito.AsyncEx;
using System.Security.Cryptography;
using System.Text;

namespace Fluffle.Ingestion.Worker.ThumbnailStorage;

public sealed class FtpThumbnailStorage : IThumbnailStorage, IDisposable, IAsyncDisposable
{
    private readonly AsyncFtpClient _ftpClient;
    private readonly AsyncLock _ftpClientLock = new();
    private readonly IOptions<FtpThumbnailStorageOptions> _options;

    public FtpThumbnailStorage(IOptions<FtpThumbnailStorageOptions> options)
    {
        _ftpClient = new AsyncFtpClient(options.Value.Host, options.Value.Username, options.Value.Password);
        _ftpClient.Config.EncryptionMode = FtpEncryptionMode.Explicit;
        _ftpClient.Config.SelfConnectMode = FtpSelfConnectMode.Always;

        _options = options;
    }

    public async Task<string> PutAsync(string itemId, Stream thumbnailStream)
    {
        if (!thumbnailStream.CanSeek)
            throw new ArgumentException("Thumbnail stream must be seekable.", nameof(thumbnailStream));

        using var _ = await _ftpClientLock.LockAsync();

        var ftpPath = GetFtpPath(itemId);
        try
        {
            var streamStartingPosition = thumbnailStream.Position;
            await _ftpClient.UploadStream(thumbnailStream, ftpPath, FtpRemoteExists.Overwrite, true);

            var checksum = await _ftpClient.GetChecksum(ftpPath, FtpHashAlgorithm.SHA1);

            thumbnailStream.Position = streamStartingPosition;
            if (!checksum.Verify(thumbnailStream))
                throw new InvalidOperationException("FTP SHA1 checksum check failed after upload.");

            var uriBuilder = new UriBuilder
            {
                Scheme = "ftp",
                Host = _options.Value.Host,
                Path = ftpPath
            };
            return uriBuilder.ToString();
        }
        catch
        {
            if (await _ftpClient.FileExists(ftpPath))
            {
                await _ftpClient.DeleteFile(ftpPath);
            }

            throw;
        }
    }

    public async Task DeleteAsync(string itemId)
    {
        using var _ = await _ftpClientLock.LockAsync();

        var ftpPath = GetFtpPath(itemId);
        if (await _ftpClient.FileExists(ftpPath))
        {
            await _ftpClient.DeleteFile(ftpPath);
        }
    }

    private string GetFtpPath(string itemId)
    {
        var saltedItemId = $"{_options.Value.Salt}:{itemId}";
        var saltedItemIdSha1Bytes = SHA1.HashData(Encoding.UTF8.GetBytes(saltedItemId));
        var itemIdHash = Convert.ToHexStringLower(saltedItemIdSha1Bytes);
        var path = Path.Join(_options.Value.Directory, $"{itemIdHash[..2]}/{itemIdHash[2..4]}/{itemIdHash}.jpg");

        return path;
    }

    public void Dispose()
    {
        _ftpClient.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _ftpClient.DisposeAsync();
    }
}

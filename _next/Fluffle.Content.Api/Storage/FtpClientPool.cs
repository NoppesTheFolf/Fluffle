using FluentFTP;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Fluffle.Content.Api.Storage;

public sealed class FtpClientPool : IDisposable
{
    private readonly ConcurrentStack<AsyncFtpClient> _ftpClients;
    private readonly SemaphoreSlim _lock;
    private readonly IOptions<FtpStorageOptions> _options;
    private readonly ILogger<FtpClientPool> _logger;

    public FtpClientPool(IOptions<FtpStorageOptions> options, ILogger<FtpClientPool> logger)
    {
        _ftpClients = new ConcurrentStack<AsyncFtpClient>();
        _lock = new SemaphoreSlim(options.Value.PoolSize, options.Value.PoolSize);
        _options = options;
        _logger = logger;
    }

    public async Task<AsyncFtpClient> RentAsync()
    {
        await _lock.WaitAsync();

        if (_ftpClients.TryPop(out var ftpClient))
        {
            _logger.LogTrace("Renting out FTP client with ID {Id}.", RuntimeHelpers.GetHashCode(ftpClient));
            return ftpClient;
        }

        _logger.LogTrace("No FTP client available from pool, creating a new one.");
        ftpClient = new AsyncFtpClient(_options.Value.Host, _options.Value.Username, _options.Value.Password);
        ftpClient.Config.EncryptionMode = FtpEncryptionMode.Explicit;
        ftpClient.Config.SelfConnectMode = FtpSelfConnectMode.Always;
        _logger.LogTrace("Created FTP client with ID {Id}.", RuntimeHelpers.GetHashCode(ftpClient));

        return ftpClient;
    }

    public void Return(AsyncFtpClient ftpClient)
    {
        _logger.LogTrace("FTP client with ID {Id} has been returned to pool.", RuntimeHelpers.GetHashCode(ftpClient));
        _ftpClients.Push(ftpClient);
        _lock.Release();
    }

    public void Dispose()
    {
        _lock.Dispose();

        foreach (var ftpClient in _ftpClients)
        {
            _logger.LogTrace("Disposing of FTP client with ID {Id}...", RuntimeHelpers.GetHashCode(ftpClient));
            ftpClient.Dispose();
        }
    }
}

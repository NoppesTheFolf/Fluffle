using System.Net;
using System.Net.Sockets;

namespace Fluffle.Search.Api.SearchByUrl;

public class SafeDownloadClient
{
    private const int MaximumSize = 4 * 1024 * 1024; // 4 MiB
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

    private readonly IHttpClientFactory _httpClientFactory;

    public SafeDownloadClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<(Stream? stream, SafeDownloadErrorCode? errorCode)> DownloadUrlAsync(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return (null, SafeDownloadErrorCode.Unparsable);
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return (null, SafeDownloadErrorCode.InvalidScheme);
        }

        List<IPAddress> ipAddresses;
        try
        {
            ipAddresses = (await Dns.GetHostAddressesAsync(uri.DnsSafeHost))
                .Where(x => x.AddressFamily is AddressFamily.InterNetwork) // Only allow IPv4 for now, later add IPv6 when it can be tested
                .ToList();
        }
        catch (SocketException e)
        {
            if (e.SocketErrorCode == SocketError.HostNotFound)
            {
                return (null, SafeDownloadErrorCode.HostNotFound);
            }

            throw;
        }

        if (ipAddresses.Count == 0)
        {
            return (null, SafeDownloadErrorCode.NoIpAddresses);
        }

        ipAddresses = ipAddresses
            .Where(x => x.IsPublic())
            .ToList();

        if (ipAddresses.Count == 0)
        {
            return (null, SafeDownloadErrorCode.NoPublicIpAddresses);
        }

        var index = Random.Shared.Next(0, ipAddresses.Count);
        var ipAddress = ipAddresses[index];

        var dnsPinnedUriBuilder = new UriBuilder(uri)
        {
            Host = ipAddress.ToString()
        };
        var dnsPinnedUri = dnsPinnedUriBuilder.Uri;

        using var httpClient = _httpClientFactory.CreateClient(nameof(SafeDownloadClient));
        using var request = new HttpRequestMessage(HttpMethod.Get, dnsPinnedUri);

        // The Host header only makes sense in HTTP/1.1, so force using that so we can pin the DNS
        request.Version = HttpVersion.Version11;
        request.Headers.Host = uri.Host;

        using var cts = new CancellationTokenSource(Timeout);
        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
        if (!response.IsSuccessStatusCode)
        {
            return (null, SafeDownloadErrorCode.NonSuccessStatusCode);
        }

        var stream = new MemoryStream();
        try
        {
            await using var httpStream = await response.Content.ReadAsStreamAsync(cts.Token);

            var buffer = new byte[8192].AsMemory();
            int bytesRead;
            while ((bytesRead = await httpStream.ReadAsync(buffer, cts.Token)) > 0)
            {
                if (stream.Length + bytesRead > MaximumSize)
                {
                    await stream.DisposeAsync();
                    return (null, SafeDownloadErrorCode.FileTooBig);
                }

                await stream.WriteAsync(buffer[..bytesRead], cts.Token);
            }

            stream.Position = 0;
            return (stream, null);
        }
        catch
        {
            await stream.DisposeAsync();
            throw;
        }
    }
}

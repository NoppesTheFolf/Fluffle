using Flurl.Http;
using Humanizer;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Sync;
using System.IO;
using System.Threading.Tasks;

namespace Noppes.Fluffle.TwitterSync;

public interface ITwitterDownloadClient
{
    Task<Stream> GetStreamAsync(string url);
}

public class TwitterDownloadClient : ApiClient, ITwitterDownloadClient
{
    public TwitterDownloadClient(string baseUrl, string userAgent) : base(baseUrl)
    {
        FlurlClient.WithHeader("User-Agent", userAgent);
    }

    public Task<Stream> GetStreamAsync(string url) => Request(url).GetStreamAsync();
}

public class TwitterDownloadClientFactory : ClientFactory<ITwitterDownloadClient>
{
    public TwitterDownloadClientFactory(FluffleConfiguration configuration) : base(configuration)
    {
    }

    public override Task<ITwitterDownloadClient> CreateAsync(int interval, string applicationName)
    {
        var client = new TwitterDownloadClient(string.Empty, Project.UserAgent(applicationName))
        {
            RateLimiter = new RequestRateLimiter(interval.Milliseconds())
        };

        return Task.FromResult((ITwitterDownloadClient)client);
    }
}

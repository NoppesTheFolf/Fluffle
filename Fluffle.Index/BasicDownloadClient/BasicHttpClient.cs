using Flurl.Http;
using Humanizer;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using System.IO;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index;

public class BasicHttpClient : ApiClient
{
    public BasicHttpClient(int interval, string applicationName) : base(null)
    {
        var userAgent = Project.UserAgent(applicationName);
        FlurlClient.WithHeader("User-Agent", userAgent);

        RateLimiter = new RequestRateLimiter(interval.Milliseconds());
    }

    public Task<Stream> GetStreamAsync(string url) => Request(url).GetStreamAsync();
}

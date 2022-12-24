using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Inkbunny.Client;
using Noppes.Fluffle.Sync;

namespace Noppes.Fluffle.Inkbunny.Sync;

internal class SyncClient : SyncClient<SyncClient, InkbunnyContentProducer, FileForSubmission>
{
    private const string ApplicationName = "inkbunny-sync";

    public SyncClient(IServiceProvider services) : base(services)
    {
    }

    private static async Task Main(string[] args) => await RunAsync(args, "Inkbunny", (conf, services) =>
    {
        var inkbunnyConf = conf.Get<InkbunnyConfiguration>();
        var inkbunnyCredentials = inkbunnyConf.Credentials;

        var userAgent = Project.UserAgent(ApplicationName);
        var client = new InkbunnyClient(inkbunnyCredentials.Username, inkbunnyCredentials.Password, userAgent);
        if (inkbunnyConf.Sync.Throttle != null)
            client.RateLimiter = new RequestRateLimiter((TimeSpan)inkbunnyConf.Sync.Throttle);

        services.AddSingleton<IInkbunnyClient>(client);
    });
}

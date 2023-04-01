using Humanizer;
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

    private static async Task Main(string[] args) => await RunAsync(args, ApplicationName.Replace("-", "_").Pascalize(), "Inkbunny", (conf, services) =>
    {
        var inkbunnyConf = conf.Get<InkbunnyConfiguration>();
        var inkbunnyCredentials = inkbunnyConf.Credentials;
        services.AddSingleton(inkbunnyConf.Sync);

        var userAgent = Project.UserAgent(ApplicationName);
        var client = new InkbunnyClient(inkbunnyCredentials.Username, inkbunnyCredentials.Password, userAgent);
        if (inkbunnyConf.Sync.ApiThrottle != null)
            client.RateLimiter = new RequestRateLimiter((TimeSpan)inkbunnyConf.Sync.ApiThrottle);

        services.AddSingleton<IInkbunnyClient>(client);
    });
}

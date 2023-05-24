using Humanizer;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Sync;
using Noppes.Fluffle.Weasyl;
using System.Threading.Tasks;

namespace Noppes.Fluffle.WeasylSync;

public class WeasylClientFactory : ClientFactory<WeasylClient>
{
    public WeasylClientFactory(FluffleConfiguration configuration) : base(configuration)
    {
    }

    public override Task<WeasylClient> CreateAsync(int interval, string applicationName)
    {
        var weasylConf = Configuration.Get<WeasylConfiguration>();

        var client = new WeasylClient("https://www.weasyl.com/", Project.UserAgent(applicationName), weasylConf.ApiKey)
        {
            RateLimiter = new RequestRateLimiter(interval.Milliseconds())
        };

        return Task.FromResult(client);
    }
}

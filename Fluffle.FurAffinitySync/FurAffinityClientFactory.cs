using Humanizer;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Sync;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurAffinitySync;

public class FurAffinityClientFactory : ClientFactory<FurAffinityClient>
{
    public FurAffinityClientFactory(FluffleConfiguration configuration) : base(configuration)
    {
    }

    public override Task<FurAffinityClient> CreateAsync(int interval, string applicationName)
    {
        var faConf = Configuration.Get<FurAffinityConfiguration>();

        var client = new FurAffinityClient("https://www.furaffinity.net", Project.UserAgent(applicationName), faConf.A, faConf.B)
        {
            RateLimiter = new RequestRateLimiter(interval.Milliseconds())
        };

        return Task.FromResult(client);
    }
}

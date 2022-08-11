using Humanizer;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Sync;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurryNetworkSync
{
    public class FurryNetworkClientFactory : ClientFactory<FurryNetworkClient>
    {
        private const string BaseUrl = "https://furrynetwork.com";

        public FurryNetworkClientFactory(FluffleConfiguration configuration) : base(configuration)
        {
        }

        public override Task<FurryNetworkClient> CreateAsync(int interval, string applicationName)
        {
            var conf = Configuration.Get<FurryNetworkConfiguration>();

            var client = new FurryNetworkClient(BaseUrl, conf.Token, Project.UserAgent(applicationName))
            {
                RateLimiter = new RequestRateLimiter(interval.Milliseconds())
            };

            return Task.FromResult(client);
        }
    }
}

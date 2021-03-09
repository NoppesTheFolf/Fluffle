using Humanizer;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
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

        public override Task<FurryNetworkClient> CreateAsync(string productName)
        {
            var conf = Configuration.Get<FurryNetworkConfiguration>();
            var contactConf = Configuration.Get<ContactConfiguration>();
            var client = new FurryNetworkClient(BaseUrl, conf.Token,
                $"{productName}/{Project.Version} (by {contactConf.Username} at {contactConf.Platform})", 2.Seconds());

            return Task.FromResult(client);
        }
    }
}

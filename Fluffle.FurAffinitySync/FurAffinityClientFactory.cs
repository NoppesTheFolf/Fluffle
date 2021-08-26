using Humanizer;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Sync;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurAffinitySync
{
    public class FurAffinityClientFactory : ClientFactory<FurAffinityClient>
    {
        public FurAffinityClientFactory(FluffleConfiguration configuration) : base(configuration)
        {
        }

        public override Task<FurAffinityClient> CreateAsync(string productName, int interval)
        {
            var faConf = Configuration.Get<FurAffinityConfiguration>();
            var contactConf = Configuration.Get<ContactConfiguration>();

            var client = new FurAffinityClient(
                "https://www.furaffinity.net",
                $"{productName}/{Project.Version} (by {contactConf.Username} at {contactConf.Platform})",
                faConf.A, faConf.B);
            client.AddInterceptor(new RequestRateLimiter(interval.Milliseconds()));

            return Task.FromResult(client);
        }
    }
}

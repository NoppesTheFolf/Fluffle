using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Sync;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index;

public class BasicClientFactory : ClientFactory<BasicHttpClient>
{
    public BasicClientFactory(FluffleConfiguration configuration) : base(configuration)
    {
    }

    public override Task<BasicHttpClient> CreateAsync(int interval, string applicationName)
    {
        return Task.FromResult(new BasicHttpClient(interval, applicationName));
    }
}

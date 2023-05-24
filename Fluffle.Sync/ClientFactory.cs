using Noppes.Fluffle.Configuration;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Sync;

public abstract class ClientFactory<TClient>
{
    protected readonly FluffleConfiguration Configuration;

    protected ClientFactory(FluffleConfiguration configuration)
    {
        Configuration = configuration;
    }

    public abstract Task<TClient> CreateAsync(int interval, string applicationName);
}

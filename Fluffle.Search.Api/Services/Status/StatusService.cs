using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.Services;

public class StatusService : Service, IStatusService
{
    private readonly FluffleClient _client;

    public StatusService(FluffleClient client)
    {
        _client = client;
    }

    public Task<IList<StatusModel>> GetStatusAsync()
    {
        return _client.GetStatusAsync();
    }
}

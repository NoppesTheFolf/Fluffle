using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Main.Communication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Services
{
    public interface IPlatformService
    {
        IEnumerable<PlatformModel> GetPlatforms();

        Task<SR<PlatformModel>> GetPlatform(string platformName);

        Task<SR<PlatformSyncModel>> GetSync(string platformName);

        Task<SE> SignalSync(string platformName, SyncTypeConstant syncType);

        Task<SR<SyncStateModel>> GetSyncState(string platformName);

        Task<SE> PutSyncState(string platformName, SyncStateModel model);
    }
}

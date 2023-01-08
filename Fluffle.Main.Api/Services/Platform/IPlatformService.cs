using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Main.Communication;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Services
{
    public static class PlatformSyncError
    {
        private const string NotFoundCode = "PLATFORM_SYNC_NOT_FOUND";

        public static SE PlatformSyncNotFound(string platformName, SyncTypeConstant syncType)
        {
            return new(NotFoundCode, HttpStatusCode.NotFound, $"No {syncType.ToString().ToLowerInvariant()} sync has been configured for {platformName}.");
        }
    }

    public interface IPlatformService
    {
        IEnumerable<PlatformModel> GetPlatforms();

        Task<SR<PlatformModel>> GetPlatform(string platformName);

        Task<SR<PlatformSyncModel>> GetSync(string platformName, SyncTypeConstant syncType);

        Task<SE> SignalSync(string platformName, SyncTypeConstant syncType);

        Task<SR<SyncStateModel>> GetSyncState(string platformName);

        Task<SE> PutSyncState(string platformName, SyncStateModel model);
    }
}

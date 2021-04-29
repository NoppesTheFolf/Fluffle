using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Main.Communication;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Services
{
    public static class PlatformError
    {
        private const string NotFoundCode = "PLATFORM_NOT_FOUND";

        public static SE PlatformNotFound(string name)
        {
            return new(NotFoundCode, HttpStatusCode.NotFound,
                $"No platform exists with name `{name}`. Make sure you're using kebab casing.");
        }
    }

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

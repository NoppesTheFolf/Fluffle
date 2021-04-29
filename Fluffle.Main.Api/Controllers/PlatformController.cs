using Microsoft.AspNetCore.Mvc;
using Noppes.Fluffle.Api.AccessControl;
using Noppes.Fluffle.Api.Controllers;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Main.Api.Services;
using Noppes.Fluffle.Main.Communication;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Controllers
{
    public class PlatformController : ApiControllerV1
    {
        public const string SingularRoute = Endpoints.Platform + "/{platformName}";

        private readonly IPlatformService _platformService;

        public PlatformController(IPlatformService platformService)
        {
            _platformService = platformService;
        }

        [HttpGet(Endpoints.Platforms)]
        [Permissions(PlatformPermissions.Read)]
        public IActionResult GetPlatforms()
        {
            var platforms = _platformService.GetPlatforms();

            return Ok(platforms);
        }

        [HttpGet(SingularRoute)]
        [Permissions(PlatformPermissions.Read)]
        public async Task<IActionResult> GetPlatform(string platformName)
        {
            var result = await _platformService.GetPlatform(platformName);

            return HandleV1(result);
        }


        [HttpGet(SingularRoute + "/sync")]
        [Permissions(PlatformPermissions.ReadSync)]
        public async Task<IActionResult> GetSync(string platformName)
        {
            var result = await _platformService.GetSync(platformName);

            return HandleV1(result);
        }

        [HttpPut(SingularRoute + "/sync/{syncType}")]
        [Permissions(PlatformPermissions.UpdateSync)]
        public async Task<IActionResult> PutSync(string platformName, SyncTypeConstant syncType)
        {
            var error = await _platformService.SignalSync(platformName, syncType);

            return HandleV1(error);
        }

        [HttpGet(SingularRoute + "/sync-state")]
        [Permissions(PlatformPermissions.ReadSyncState)]
        public async Task<IActionResult> GetSyncState(string platformName)
        {
            var result = await _platformService.GetSyncState(platformName);

            return HandleV1(result);
        }

        [HttpPut(SingularRoute + "/sync-state")]
        [Permissions(PlatformPermissions.PutSyncState)]
        public async Task<IActionResult> PutSyncState(string platformName, SyncStateModel model)
        {
            var error = await _platformService.PutSyncState(platformName, model);

            return HandleV1(error);
        }
    }

    public class PlatformPermissions : Permissions
    {
        private const string Prefix = "PLATFORMS_";

        [Permission]
        public const string Read = Prefix + "READ";

        [Permission]
        public const string ReadSync = Prefix + "SYNC_READ";

        [Permission]
        public const string UpdateSync = Prefix + "SYNC_UPDATE";

        [Permission]
        public const string ReadSyncState = Prefix + "READ_SYNC_STATE";

        [Permission]
        public const string PutSyncState = Prefix + "PUT_SYNC_STATE";
    }
}

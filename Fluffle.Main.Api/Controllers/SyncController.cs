using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Api.AccessControl;
using Noppes.Fluffle.Api.Controllers;
using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Main.Database;
using Noppes.Fluffle.Main.Database.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Controllers
{
    public class SyncController : ApiControllerV1
    {
        private const int ImagesLimit = 2500;
        private const int CreditableEntitiesLimit = 10000;

        public const string SyncRoute = Endpoints.Sync;

        private readonly FluffleContext _context;

        public SyncController(FluffleContext context)
        {
            _context = context;
        }

        [HttpGet(SyncRoute + "/images/{afterChangeId}")]
        [Permissions(SyncPermissions.ReadImages)]
        public async Task<IActionResult> GetImages(int afterChangeId)
        {
            var changes = await _context.Images
                .AsNoTracking()
                .IncludeThumbnails()
                .Include(i => i.Rating)
                .Include(i => i.ImageHash)
                .Include(i => i.Credits)
                .Include(i => i.Files)
                .AfterChangeId(afterChangeId)
                .Take(ImagesLimit)
                .ToListAsync();

            var maxId = changes.Max(i => i.ChangeId) ?? afterChangeId;

            return Ok(new ImagesSyncModel
            {
                NextChangeId = maxId,
                Results = changes.MapEnumerableTo<ImagesSyncModel.ImageModel>()
            });
        }

        [HttpGet(SyncRoute + "/creditable-entities/{afterChangeId}")]
        [Permissions(SyncPermissions.ReadCreditableEntities)]
        public async Task<IActionResult> GetCreditableEntities(int afterChangeId)
        {
            var changes = await _context.CreditableEntities
                .AsNoTracking()
                .AfterChangeId(afterChangeId)
                .Take(CreditableEntitiesLimit)
                .ToListAsync();

            var maxId = changes.Max(i => i.ChangeId) ?? afterChangeId;

            return Ok(new CreditableEntitiesSyncModel
            {
                NextChangeId = maxId,
                Results = changes.MapEnumerableTo<CreditableEntitiesSyncModel.CreditableEntityModel>()
            });
        }
    }

    public class SyncPermissions : Permissions
    {
        public const string Prefix = "SYNC_";

        [Permission]
        public const string ReadImages = Prefix + "READ_IMAGES";

        [Permission]
        public const string ReadCreditableEntities = Prefix + "READ_CREDITABLE_ENTITIES";
    }
}

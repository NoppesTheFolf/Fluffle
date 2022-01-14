using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Api.AccessControl;
using Noppes.Fluffle.Api.Controllers;
using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Main.Api.Services;
using Noppes.Fluffle.Main.Database.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Controllers
{
    public class CreditableEntityController : ApiControllerV1
    {
        private readonly FluffleContext _context;

        public CreditableEntityController(FluffleContext context)
        {
            _context = context;
        }

        [HttpGet(PlatformController.SingularRoute + "/creditable-entity/{creditableEntityName}/priorities/max")]
        [Permissions(CreditableEntityPermissions.Read)]
        public async Task<IActionResult> GetMaxPriority(string platformName, string creditableEntityName)
        {
            var result = await _context.Platforms.GetPlatformAsync(platformName, async platform =>
            {
                var maxPriority = await _context.CreditableEntities
                    .Where(ce => ce.PlatformId == platform.Id && ce.IdOnPlatform == creditableEntityName)
                    .SelectMany(ce => ce.Content.Select(c => c.Priority))
                    .OrderByDescending(p => p)
                    .Cast<int?>()
                    .FirstOrDefaultAsync();

                return new SR<int?>(maxPriority);
            });

            return HandleV1(result);
        }
    }

    public class CreditableEntityPermissions : Permissions
    {
        private const string Prefix = "CREDITABLE_ENTITY_";

        [Permission]
        public const string Read = Prefix + "READ";
    }
}

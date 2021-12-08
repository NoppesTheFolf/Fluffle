using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Api.AccessControl;
using Noppes.Fluffle.Api.Controllers;
using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Main.Database.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Controllers
{
    public class SourcesController : ApiControllerV1
    {
        public const string PluralRoute = Endpoints.Sources;

        private readonly FluffleContext _context;

        public SourcesController(FluffleContext context)
        {
            _context = context;
        }

        [HttpGet(PluralRoute + "/other/{afterId}")]
        [Permissions(SourcesPermissions.ReadImages)]
        public async Task<IActionResult> GetStatus(int afterId)
        {
            var otherSources = await _context.ContentOtherSources
                .Where(x => x.Id > afterId)
                .OrderBy(x => x.Id)
                .Take(Endpoints.SourcesLimit)
                .ToListAsync();

            return Ok(otherSources.MapEnumerableTo<OtherSourceModel>().ToList());
        }
    }

    public class SourcesPermissions : Permissions
    {
        public const string Prefix = "SOURCES_";

        [Permission]
        public const string ReadImages = Prefix + "READ_OTHER_SOURCES";
    }
}

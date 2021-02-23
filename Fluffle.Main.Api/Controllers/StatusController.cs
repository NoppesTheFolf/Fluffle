using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Api.AccessControl;
using Noppes.Fluffle.Api.Controllers;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Main.Database.Models;
using System.Linq;

namespace Noppes.Fluffle.Main.Api.Controllers
{
    public class StatusController : ApiControllerV1
    {
        private const string Singular = "status";

        private readonly FluffleContext _context;

        public StatusController(FluffleContext context)
        {
            _context = context;
        }

        [HttpGet(Singular), Permissions(StatusPermissions.View)]
        public IActionResult GetStatus()
        {
            var statusModels = _context.Platforms
                .Include(p => p.IndexStatistics)
                .Select(p => new StatusModel
                {
                    Name = p.Name,
                    EstimatedCount = p.EstimatedContentCount,
                    StoredCount = p.IndexStatistics.Where(iss => iss.MediaTypeId == (int)MediaTypeConstant.Image).Sum(s => s.Count),
                    IndexedCount = p.IndexStatistics.Where(iss => iss.MediaTypeId == (int)MediaTypeConstant.Image).Sum(s => s.IndexedCount),
                    IsComplete = p.IsComplete
                }).OrderBy(s => s.Name)
                .AsEnumerable()
                .Select(s =>
                {
                    if (s.StoredCount > s.EstimatedCount)
                        s.EstimatedCount = s.StoredCount;

                    return s;
                });

            return Ok(statusModels);
        }
    }

    public class StatusPermissions : Permissions
    {
        public const string Prefix = "STATUS_";

        [Permission]
        public const string View = Prefix + "VIEW";
    }
}

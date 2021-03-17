using Microsoft.AspNetCore.Mvc;
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
        private IndexStatisticsService _iss;

        public StatusController(FluffleContext context, IndexStatisticsService iss)
        {
            _context = context;
            _iss = iss;
        }

        [HttpGet(Singular), Permissions(StatusPermissions.View)]
        public IActionResult GetStatus()
        {
            var statusModels = _context.Platforms
                .AsEnumerable()
                .Select(p =>
                {
                    var (total, indexed) = _iss.Get(p.Id, (int)MediaTypeConstant.Image);

                    return new StatusModel
                    {
                        Name = p.Name,
                        EstimatedCount = p.EstimatedContentCount,
                        StoredCount = total,
                        IndexedCount = indexed,
                        IsComplete = p.IsComplete
                    };
                }).OrderBy(s => s.Name)
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

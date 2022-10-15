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
        private readonly IndexStatisticsService _iss;

        public StatusController(FluffleContext context, IndexStatisticsService iss)
        {
            _context = context;
            _iss = iss;
        }

        [HttpGet(Singular), Permissions(StatusPermissions.View)]
        public IActionResult GetStatus()
        {
            using var scope = _iss.Scope();

            var models = _context.Platforms
                .AsEnumerable()
                .Select(p =>
                {
                    var (total, indexed, historyLast30Days, historyLast24Hours) =
                        scope.Get(p.Id, (int)MediaTypeConstant.Image, (int)MediaTypeConstant.AnimatedImage);

                    var model = new StatusModel
                    {
                        Name = p.Name,
                        EstimatedCount = total > p.EstimatedContentCount ? total : p.EstimatedContentCount,
                        StoredCount = total,
                        IndexedCount = indexed,
                        IsComplete = p.IsComplete || p.EstimatedContentCount == -1,
                        HistoryLast30Days = historyLast30Days,
                        HistoryLast24Hours = historyLast24Hours
                    };

                    return model;
                })
                .OrderBy(m => m.Name)
                .ToList();

            return Ok(models);
        }
    }

    public class StatusPermissions : Permissions
    {
        public const string Prefix = "STATUS_";

        [Permission]
        public const string View = Prefix + "VIEW";
    }
}

using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Main.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Services
{
    public class PlatformService : Service, IPlatformService
    {
        private readonly FluffleContext _context;

        public PlatformService(FluffleContext context)
        {
            _context = context;
        }

        public IEnumerable<PlatformModel> GetPlatforms()
        {
            return _context.Platforms
                .MapEnumerableTo<PlatformModel>();
        }

        public Task<SR<PlatformModel>> GetPlatform(string platformName)
        {
            return _context.Platforms.GetPlatformAsync(platformName, platform =>
            {
                var model = platform.MapTo<PlatformModel>();

                return Task.FromResult(new SR<PlatformModel>(model));
            });
        }

        public async Task<SR<PlatformSyncModel>> GetSync(string platformName)
        {
            var query = _context.Platforms
                .Include(p => p.PlatformSyncs);

            return await query.GetPlatformAsync(platformName, platform =>
            {
                var now = DateTime.UtcNow;

                var models = platform.PlatformSyncs
                    .Select(ps => new PlatformSyncModel.SyncInfo
                    {
                        Type = (SyncTypeConstant)ps.SyncTypeId,
                        When = ps.When.ToUniversalTime(),
                        TimeToWait = ps.Interval - (now - ps.When.ToUniversalTime())
                    })
                    .Select(m =>
                    {
                        if (m.TimeToWait.Ticks < 0)
                            m.TimeToWait = TimeSpan.Zero;

                        return m;
                    })
                    .OrderBy(m => m.TimeToWait)
                    .ThenBy(m => m.Type)
                    .ToList();

                var model = new PlatformSyncModel();
                model.Next = models.FirstOrDefault();
                model.Other = models.Where(m => m != model.Next).ToList();

                var result = new SR<PlatformSyncModel>(model);
                return Task.FromResult(result);
            });
        }

        public async Task<SE> SignalSync(string platformName, SyncTypeConstant syncType)
        {
            return await _context.Platforms.GetPlatformAsync(platformName, async platform =>
            {
                var syncTypeId = (int)syncType;
                var sync = await _context.PlatformSyncs
                    .FirstAsync(ps => ps.PlatformId == platform.Id && ps.SyncTypeId == syncTypeId);

                sync.When = DateTime.UtcNow;

                // A full sync completing means the database is synchronized in its entirety and
                // therefore complete
                if (syncType == SyncTypeConstant.Full)
                    platform.IsComplete = true;

                await _context.SaveChangesAsync();

                return null;
            });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Api.Database;
using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Main.Database.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Services;

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

    public async Task<SR<PlatformSyncModel>> GetSync(string platformName, SyncTypeConstant constant)
    {
        return await _context.Platforms.GetPlatformAsync(platformName, async platform =>
        {
            var platformSync = await _context.PlatformSyncs
                .FirstOrDefaultAsync(x => x.PlatformId == platform.Id && x.SyncTypeId == (int)constant);

            if (platformSync == null)
                return new SR<PlatformSyncModel>(PlatformSyncError.PlatformSyncNotFound(platform.Name, constant));

            var now = DateTime.UtcNow;
            var when = platformSync.When.ToUniversalTime();
            var timePassedSinceLastSync = now - when;
            var ttw = platformSync.Interval - timePassedSinceLastSync;
            ttw = ttw < TimeSpan.Zero ? TimeSpan.Zero : ttw;

            return new SR<PlatformSyncModel>(new PlatformSyncModel
            {
                When = when,
                TimeToWait = ttw
            });
        });
    }

    public async Task<SE> SignalSync(string platformName, SyncTypeConstant syncType)
    {
        return await _context.Platforms.GetPlatformAsync(platformName, async platform =>
        {
            var syncTypeId = (int)syncType;
            var sync = await _context.PlatformSyncs
                .FirstAsync(ps => ps.PlatformId == platform.Id && ps.SyncTypeId == syncTypeId);

            // Any type of finished synchronization process indicates the database has at least
            // been synchronized in its entirety once.
            platform.IsComplete = true;
            sync.When = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return null;
        });
    }

    public async Task<SR<SyncStateModel>> GetSyncState(string platformName)
    {
        return await _context.Platforms.Include(p => p.SyncState).GetPlatformAsync(platformName, platform =>
        {
            var model = platform.SyncState == null ? null : new SyncStateModel
            {
                Version = platform.SyncState.Version,
                Document = platform.SyncState.Document
            };

            return Task.FromResult(new SR<SyncStateModel>(model));
        });
    }

    public async Task<SE> PutSyncState(string platformName, SyncStateModel model)
    {
        return await _context.Platforms.Include(p => p.SyncState).GetPlatformAsync(platformName, async platform =>
        {
            platform.SyncState ??= new SyncState
            {
                Id = platform.Id
            };
            platform.SyncState.Document = model.Document;
            platform.SyncState.Version = model.Version;

            await _context.SaveChangesAsync();
            return null;
        });
    }
}

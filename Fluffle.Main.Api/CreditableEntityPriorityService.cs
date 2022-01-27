using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Api.RunnableServices;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Main.Api.Helpers;
using Noppes.Fluffle.Main.Database.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api
{
    public class CreditableEntityPriorityService : IService
    {
        private const int BatchSize = 10_000;

        private readonly MainServerConfiguration _configuration;
        private readonly IServiceProvider _services;
        private readonly ILogger<CreditableEntityPriorityService> _logger;
        private readonly ChangeIdIncrementer<CreditableEntity> _cci;

        public CreditableEntityPriorityService(MainServerConfiguration configuration, IServiceProvider services, ILogger<CreditableEntityPriorityService> logger, ChangeIdIncrementer<CreditableEntity> cci)
        {
            _configuration = configuration;
            _services = services;
            _logger = logger;
            _cci = cci;
        }

        public async Task RunAsync()
        {
            while (true)
            {
                using var scope = _services.CreateScope();
                await using var context = scope.ServiceProvider.GetRequiredService<FluffleContext>();

                var now = DateTime.UtcNow;
                var creditableEntities = await context.CreditableEntities
                    .OrderByDescending(ce => ce.PriorityUpdatedAt == null)
                    .ThenBy(ce => ce.PriorityUpdatedAt)
                    .Take(BatchSize)
                    .ToListAsync();

                creditableEntities = creditableEntities
                    .Where(ce => ce.PriorityUpdatedAt == null || ce.PriorityUpdatedAt < now.Subtract(_configuration.CreditableEntityPriorityExpirationTime.Minutes()))
                    .ToList();

                if (creditableEntities.Count == 0)
                    break;

                _logger.LogInformation("Calculating priorities for {count} creditable entities.", creditableEntities.Count);

                var creditableEntitiesStats = await context.ContentCreditableEntities
                    .Where(cce => creditableEntities.Select(ce => ce.Id).Contains(cce.CreditableEntityId))
                    .GroupBy(cce => cce.CreditableEntityId)
                    .Select(g => new
                    {
                        Id = g.Key,
                        Priority = g.Max(cce => cce.Content.Priority)
                    }).ToDictionaryAsync(x => x.Id, x => (int?)x.Priority);

                foreach (var creditableEntity in creditableEntities)
                {
                    creditableEntitiesStats.TryGetValue(creditableEntity.Id, out var priority);
                    creditableEntity.Priority = priority ?? int.MinValue;

                    var creditableEntityState = context.Entry(creditableEntity).State;
                    if (creditableEntityState == EntityState.Detached)
                        throw new InvalidOperationException();

                    if (creditableEntityState is EntityState.Modified)
                    {
                        using var _ = _cci.Lock((PlatformConstant)creditableEntity.PlatformId, out var incrementer);
                        incrementer.Next(creditableEntity);
                    }

                    creditableEntity.PriorityUpdatedAt = now;
                }

                await context.SaveChangesAsync();

                if (creditableEntities.Count < BatchSize)
                    break;
            }
        }
    }
}

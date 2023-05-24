using Noppes.Fluffle.Database.Synchronization;
using Noppes.Fluffle.Main.Api.Helpers;
using Noppes.Fluffle.Main.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Services;

public static class CreditableEntityExtensions
{
    public static async Task<SynchronizeResult<CreditableEntity>> SynchronizeCreditableEntitiesAsync(
        this FluffleContext context, ICollection<CreditableEntity> currentCreditableEntities,
        ICollection<CreditableEntity> newCreditableEntities, ChangeIdIncrementerLock<CreditableEntity> cid)
    {
        return await context.SynchronizeAsync(c => c.CreditableEntities, currentCreditableEntities,
            newCreditableEntities, (f1, f2) =>
            {
                return f1.Id == f2.Id;
            }, entity =>
           {
               cid.Next(entity);

               return Task.CompletedTask;
           }, (src, dest) =>
           {
               dest.Name = src.Name;
               dest.Type = src.Type;

               return Task.CompletedTask;
           }, (src, dest) =>
           {
               cid.Next(dest);

               return Task.CompletedTask;
           }
        );
    }
}

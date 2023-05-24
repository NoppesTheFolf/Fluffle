using Noppes.Fluffle.Database.Synchronization;
using Noppes.Fluffle.Main.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Services;

public static class ContentCreditableEntityExtensions
{
    public static async Task<SynchronizeResult<ContentCreditableEntity>> SynchronizeContentCreditsAsync(
        this FluffleContext context, ICollection<ContentCreditableEntity> currentCredits, ICollection<ContentCreditableEntity> newCredits)
    {
        return await context.SynchronizeAsync(c => c.ContentCreditableEntities, currentCredits, newCredits, (c1, c2) =>
        {
            return (c1.Content, c1.CreditableEntity) == (c2.Content, c2.CreditableEntity);
        });
    }
}

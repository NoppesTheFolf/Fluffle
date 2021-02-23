using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Database.Synchronization
{
    public static class SynchronizationExtensions
    {
        private class StateTracker<TEntity>
        {
            public TEntity Entity { get; init; }

            public bool HasBeenUpdated { get; set; }
        }

        public static async Task<SynchronizeResult<TEntity>> SynchronizeAsync<TContext, TEntity>(this TContext context,
            Func<TContext, DbSet<TEntity>> selectSet, ICollection<TEntity> currentEntities,
            ICollection<TEntity> newEntities, Func<TEntity, TEntity, bool> match,
            Func<TEntity, Task> onInsertAsync = null, Func<TEntity, TEntity, Task> onUpdateAsync = null,
            Func<TEntity, TEntity, Task> onUpdateChangesAsync = null, Func<TEntity, TEntity, Task> updateAnywayAsync = null) where TContext : DbContext where TEntity : class
        {
            var result = new SynchronizeResult<TEntity>();
            var set = selectSet(context);

            var trackedCurrentEntities = currentEntities
                .Select(ce => new StateTracker<TEntity>
                {
                    Entity = ce,
                    HasBeenUpdated = false
                }).ToList();

            if (currentEntities.Any(e => context.Entry(e).State == EntityState.Detached))
                throw new InvalidOperationException("Hmmmm, you provided entities that aren't being tracked. Did you do a switcharoo with the parameters?");

            if (!trackedCurrentEntities.Any())
            {
                foreach (var newEntity in newEntities)
                {
                    if (onInsertAsync != null)
                        await onInsertAsync(newEntity);

                    result.Added.Add(new EntitySynchronizeResult<TEntity>(newEntity, true));
                }

                if (set != null)
                    await set.AddRangeAsync(newEntities);

                return result;
            }

            // Update existing entities
            var entitiesToAdd = new List<TEntity>();
            foreach (var newEntity in newEntities)
            {
                var entityTracker = trackedCurrentEntities
                    .FirstOrDefault(se => match(newEntity, se.Entity));

                if (entityTracker == null)
                {
                    entitiesToAdd.Add(newEntity);
                    continue;
                }

                if (onUpdateAsync != null)
                    await onUpdateAsync(newEntity, entityTracker.Entity);

                entityTracker.HasBeenUpdated = true;

                var finalResult = new EntitySynchronizeResult<TEntity>(entityTracker.Entity);
                result.Updated.Add(finalResult);

                var entry = context.Entry(entityTracker.Entity);
                if (entry.State == EntityState.Modified)
                {
                    finalResult.HasChanges = true;

                    if (onUpdateChangesAsync != null)
                        await onUpdateChangesAsync(newEntity, entityTracker.Entity);
                }

                if (updateAnywayAsync != null)
                    await updateAnywayAsync(newEntity, entityTracker.Entity);
            }

            // Delete entities that didn't get updated
            var deletedEntities = trackedCurrentEntities
                .Where(e => !e.HasBeenUpdated)
                .Select(e => e.Entity)
                .ToList();

            if (deletedEntities.Any())
            {
                set?.RemoveRange(deletedEntities);

                deletedEntities.ForEach(result.Removed.Add);
            }

            // Lastly add the new entities
            if (entitiesToAdd.Any())
            {
                foreach (var entityToAdd in entitiesToAdd)
                {
                    if (onInsertAsync != null)
                        await onInsertAsync(entityToAdd);

                    result.Added.Add(new EntitySynchronizeResult<TEntity>(entityToAdd, true));
                }

                if (set != null)
                    await set.AddRangeAsync(entitiesToAdd);
            }

            return result;
        }
    }
}

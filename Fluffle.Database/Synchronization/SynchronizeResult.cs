using System.Collections.Generic;
using System.Linq;

namespace Noppes.Fluffle.Database.Synchronization
{
    public class SynchronizeResult<TEntity>
    {
        public bool HasChanges
        {
            get
            {
                if (Added.Any() || Removed.Any())
                    return true;

                return Updated.Any(r => r.HasChanges);
            }
        }

        public ICollection<EntitySynchronizeResult<TEntity>> Added { get; set; }

        public ICollection<EntitySynchronizeResult<TEntity>> Updated { get; set; }

        public ICollection<TEntity> Removed { get; set; }

        public IEnumerable<EntitySynchronizeResult<TEntity>> Results() => Added.Concat(Updated);

        public IEnumerable<TEntity> Entities() =>
            Added.Select(r => r.Entity).Concat(Updated.Select(r => r.Entity));

        public SynchronizeResult()
        {
            Added = new List<EntitySynchronizeResult<TEntity>>();
            Updated = new List<EntitySynchronizeResult<TEntity>>();
            Removed = new List<TEntity>();
        }
    }
}

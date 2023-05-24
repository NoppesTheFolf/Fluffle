namespace Noppes.Fluffle.Database.Synchronization;

public class EntitySynchronizeResult<TEntity>
{
    public TEntity Entity { get; set; }

    public bool HasChanges { get; set; }

    public EntitySynchronizeResult(TEntity entity, bool hasChanges = false)
    {
        Entity = entity;
        HasChanges = hasChanges;
    }
}

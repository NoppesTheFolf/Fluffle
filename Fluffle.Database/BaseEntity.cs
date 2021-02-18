using System;

namespace Noppes.Fluffle.Database
{
    /// <summary>
    /// The base class for entities. Used for marking a class as an entity so that it can be
    /// discovered by reflection. Probably could have just used an attribute here instead...
    /// </summary>
    public abstract class BaseEntity
    {
    }

    /// <summary>
    /// Keeping track of when an entity was created/updated might come in handy in some scenarios.
    /// Like, let's say, rolling back certain entities because they were created after a defect got introduced.
    /// </summary>
    public abstract class TrackedBaseEntity : BaseEntity
    {
        /// <summary>
        /// When the entity got created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the entity got updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}

using System;

namespace Noppes.Fluffle.Database
{
    /// <summary>
    /// Marks a property an database entity to be 'synchronizable'. This means the value provided in
    /// the seeders need to be in sync with those stored in the database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SyncAttribute : Attribute
    {
    }
}

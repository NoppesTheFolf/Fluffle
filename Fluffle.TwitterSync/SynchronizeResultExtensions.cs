using Noppes.Fluffle.Database.Synchronization;
using Serilog;
using System.Linq;

namespace Noppes.Fluffle.TwitterSync;

public static class SynchronizeResultExtensions
{
    public static void Print<T>(this SynchronizeResult<T> result)
    {
        Log.Information("Results for synchronization for entities of type {type}: A{added} U{updated} C{changes} D{deleted}", typeof(T).Name, result.Added.Count, result.Updated.Count, result.Updated.Count(u => u.HasChanges), result.Removed.Count);
    }
}

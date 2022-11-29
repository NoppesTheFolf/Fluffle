using Noppes.Fluffle.DeviantArt.Client.Models;

namespace Noppes.Fluffle.DeviantArt.QueryDeviationsWatcher;

public class QueryResult
{
    public string Query { get; }

    public ICollection<Deviation> Deviations { get; }

    public QueryResult(string query, ICollection<Deviation> deviations)
    {
        Query = query;
        Deviations = deviations;
    }
}

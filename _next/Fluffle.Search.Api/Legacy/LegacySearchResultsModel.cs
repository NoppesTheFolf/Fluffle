namespace Fluffle.Search.Api.Legacy;

public class LegacySearchResultsModel
{
    public required string Id { get; set; }

    public required LegacySearchResultStatsModel Stats { get; set; }

    public required IList<LegacySearchResultModel> Results { get; set; }
}

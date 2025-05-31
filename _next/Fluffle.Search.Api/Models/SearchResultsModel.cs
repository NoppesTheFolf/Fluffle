namespace Fluffle.Search.Api.Models;

public class SearchResultsModel
{
    public required string Id { get; set; }

    public required IList<SearchResultModel> Results { get; set; }
}

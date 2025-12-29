namespace Fluffle.Search.Api.Models;

public class SearchByUrlModel
{
    public required string Url { get; set; }

    public required int Limit { get; set; }
}

namespace Fluffle.Search.Api.Models;

public class SearchByIdModel
{
    public required string Id { get; set; }

    public required int Limit { get; set; }
}

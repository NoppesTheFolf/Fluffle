namespace Fluffle.Search.Api.Models;

public class SearchModel
{
    public required IFormFile Image { get; set; }

    public required int Limit { get; set; }
}

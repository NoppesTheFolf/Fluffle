namespace Fluffle.Search.Api.Models;

public class SearchModel
{
    public required IFormFile File { get; set; }

    public required int Limit { get; set; }

    public bool CreateLink { get; set; }
}

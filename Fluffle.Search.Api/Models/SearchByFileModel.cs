namespace Fluffle.Search.Api.Models;

public class SearchByFileModel
{
    public required IFormFile File { get; set; }

    public required int Limit { get; set; }
}

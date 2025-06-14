namespace Fluffle.Search.Api.Models;

public class SearchResultModel
{
    public required string Id { get; set; }

    public required float Distance { get; set; }

    public required SearchResultModelMatch Match { get; set; }

    public required string Platform { get; set; }

    public required string Url { get; set; }

    public required bool IsSfw { get; set; }

    public required SearchResultThumbnailModel? Thumbnail { get; set; }

    public required ICollection<SearchResultAuthorModel> Authors { get; set; }
}

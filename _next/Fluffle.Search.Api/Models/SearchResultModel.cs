namespace Fluffle.Search.Api.Models;

public class SearchResultModel
{
    public required string Id { get; set; }

    public required float Score { get; set; }

    public required string Platform { get; set; }

    public required string Url { get; set; }

    public required bool IsSfw { get; set; }

    public required SearchResultThumbnailModel? Thumbnail { get; set; }

    public required ICollection<SearchResultAuthorModel> Authors { get; set; }
}

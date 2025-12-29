namespace Fluffle.Search.Api.Legacy;

public class LegacySearchResultModel
{
    public required int Id { get; set; }

    public required float Score { get; set; }

    public required LegacySearchResultMatchModel Match { get; set; }

    public required string Platform { get; set; }

    public required string Location { get; set; }

    public required bool IsSfw { get; set; }

    public required LegacySearchResultThumbnailModel? Thumbnail { get; set; }

    public required ICollection<LegacySearchResultCreditModel> Credits { get; set; }
}

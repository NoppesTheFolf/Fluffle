namespace Fluffle.Search.Api.Legacy;

public class LegacySearchModel
{
    public IFormFile? File { get; set; } = null;

    public bool IncludeNsfw { get; set; } = false;

    public ICollection<string>? Platforms { get; set; } = null;

    public int Limit { get; set; } = 32;
}

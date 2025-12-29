using System.ComponentModel;

namespace Fluffle.Search.Api.Models;

public class SearchResultAuthorModel
{
    public required string Id { get; set; }

    [Description("The interpretation of this field is somewhat dependent on the platform from which the image was scraped. " +
                 "For e621, it's based on the artist tags and it's therefore safe to assume this field includes the names of the artist(s) that created the artwork. " +
                 "For all other platforms, it's the name of user that uploaded said image, which might be the artist, a commissioner, etc.")]
    public required string Name { get; set; }
}

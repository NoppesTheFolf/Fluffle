using Newtonsoft.Json;
using System.Collections.Generic;

namespace Noppes.Fluffle.FurryNetworkSync;

public class FnToken
{
    public string AccessToken { get; set; }

    [JsonProperty("expires_in")]
    public int ExpiresInSeconds { get; set; }
}

public class FnSubmissionImagesModel
{
    public string Strip { get; set; }

    public string Small { get; set; }

    public string Large { get; set; }

    public string Medium { get; set; }

    public string Original { get; set; }

    public string Thumbnail { get; set; }

    public string ThumbnailSmall { get; set; }
}

public class FnSubmission
{
    public int Id { get; set; }

    public int Favorites { get; set; }

    public FnSubmissionImagesModel Images { get; set; }

    public ICollection<string> Tags { get; set; }

    public FnSubmissionRating Rating { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string RecordType { get; set; }

    public string ContentType { get; set; }

    public FnCharacter Character { get; set; }
}

public class FnSpecificSubmission : FnSubmission
{
    public new ICollection<FnTag> Tags { get; set; }
}

public class FnTag
{
    public string Value { get; set; }
}

public class FnCharacter
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string DisplayName { get; set; }
}

public enum FnSubmissionRating
{
    General = 0,
    Mature = 1,
    Explicit = 2
}

public class FnSearchResult
{
    public ICollection<FnSubmission> After { get; set; }

    public ICollection<FnSubmission> Before { get; set; }

    public ICollection<FnSubmission> Hits { get; set; }

    public int Total { get; set; }
}

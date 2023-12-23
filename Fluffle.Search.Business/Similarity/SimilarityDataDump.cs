namespace Noppes.Fluffle.Search.Business.Similarity;

public class SimilarityDataDump
{
    public string Id { get; set; } = null!;

    public DateTime When { get; set; }

    public ICollection<PlatformSimilarityDataDump> Platforms { get; set; } = null!;
}

public class PlatformSimilarityDataDump
{
    public int PlatformId { get; set; }

    public long ChangeId { get; set; }

    public HashCollectionPlatformSimilarityDataDump Sfw { get; set; } = null!;

    public HashCollectionPlatformSimilarityDataDump Nsfw { get; set; } = null!;
}

public class HashCollectionPlatformSimilarityDataDump
{
    public string FileName { get; set; } = null!;

    public string Md5 { get; set; } = null!;
}

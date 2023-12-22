namespace Noppes.Fluffle.Search.Business.Similarity;

internal class PlatformSimilarityData
{
    public int PlatformId { get; set; }

    public long ChangeId { get; set; }

    public IHashCollection SfwCollection { get; set; } = null!;

    public IHashCollection NsfwCollection { get; set; } = null!;
}

namespace Noppes.Fluffle.Search.Business.Similarity;

public class SimilarityResult
{
    public int Count { get; }

    public ICollection<NearestNeighborsResult> Images { get; }

    public SimilarityResult(int count, ICollection<NearestNeighborsResult> images)
    {
        Count = count;
        Images = images;
    }
}

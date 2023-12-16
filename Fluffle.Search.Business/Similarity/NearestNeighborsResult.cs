namespace Noppes.Fluffle.Search.Business.Similarity;

public class NearestNeighborsResult
{
    public int Id { get; }

    public int MismatchCount { get; }

    public NearestNeighborsResult(int id, int mismatchCount)
    {
        Id = id;
        MismatchCount = mismatchCount;
    }
}
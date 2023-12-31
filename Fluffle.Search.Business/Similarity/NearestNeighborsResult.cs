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

public class NearestNeighborsResultComparer : IComparer<NearestNeighborsResult>
{
    public int Compare(NearestNeighborsResult? x, NearestNeighborsResult? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return 1;
        if (ReferenceEquals(null, x)) return -1;
        var mismatchCountComparison = x.MismatchCount.CompareTo(y.MismatchCount);
        if (mismatchCountComparison != 0) return mismatchCountComparison;
        return x.Id.CompareTo(y.Id);
    }
}

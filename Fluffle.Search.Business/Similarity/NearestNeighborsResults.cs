namespace Noppes.Fluffle.Search.Business.Similarity;

public class NearestNeighborsResults
{
    /// <summary>
    /// The number of 64-bit hashes that were searched through.
    /// </summary>
    public int Count64 { get; }

    /// <summary>
    /// The number of 256-bit hashes that were searched through.
    /// </summary>
    public int Count256 { get; }

    public ICollection<NearestNeighborsResult> Results { get; }

    public NearestNeighborsResults(int count64, int count256, ICollection<NearestNeighborsResult> results)
    {
        Count64 = count64;
        Count256 = count256;
        Results = results;
    }
}

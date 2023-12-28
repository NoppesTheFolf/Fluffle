namespace Noppes.Fluffle.Search.Business.Similarity;

public readonly struct NearestNeighborsStats
{
    /// <summary>
    /// The number of 64-bit hashes that were searched through.
    /// </summary>
    public int Count64 { get; }

    /// <summary>
    /// The number of 256-bit hashes that were searched through.
    /// </summary>
    public int Count256 { get; }

    public NearestNeighborsStats(int count64, int count256)
    {
        Count64 = count64;
        Count256 = count256;
    }
}

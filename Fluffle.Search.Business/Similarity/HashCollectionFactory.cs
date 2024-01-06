namespace Noppes.Fluffle.Search.Business.Similarity;

internal static class HashCollectionFactory
{
    private const int ShardsCount = 1024;

    public static IHashCollection Create()
    {
        return new ConcurrentHashCollection(new ShardedHashCollection(ShardsCount));
    }
}

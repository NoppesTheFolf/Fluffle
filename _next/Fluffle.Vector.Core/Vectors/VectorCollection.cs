using Fluffle.Vector.Core.Domain.Vectors;
using System.IO.Hashing;
using System.Text;

namespace Fluffle.Vector.Core.Vectors;

public class VectorCollection
{
    private const int PartitionCount = 256;
    private readonly VectorCollectionPartition[] _partitions;

    public VectorCollection()
    {
        _partitions = new VectorCollectionPartition[PartitionCount];
        for (var i = 0; i < PartitionCount; i++)
        {
            _partitions[i] = new VectorCollectionPartition();
        }
    }

    public void Add(string itemId, float[] vector)
    {
        var i = XxHash32.HashToUInt32(Encoding.UTF8.GetBytes(itemId)) % PartitionCount;
        _partitions[i].Add(itemId, vector);
    }

    public void Remove(string itemId)
    {
        var i = XxHash32.HashToUInt32(Encoding.UTF8.GetBytes(itemId)) % PartitionCount;
        _partitions[i].Remove(itemId);
    }

    public IList<VectorSearchResult> Search(float[] query, int limit)
    {
        var results = _partitions
            .SelectMany(x => x.Search(query, limit))
            .OrderBy(x => x.Distance)
            .Take(limit)
            .ToList();

        return results;
    }
}

using Fluffle.Vector.Core.Domain.Vectors;
using System.Numerics.Tensors;

namespace Fluffle.Vector.Core.Vectors;

public class VectorCollectionPartition
{
    private readonly List<string> _itemIds = [];
    private readonly List<float[]> _vectors = [];
    private readonly ReaderWriterLockSlim _lock = new();

    public void Add(string itemId, float[] vector)
    {
        Manipulate(itemId, i =>
        {
            _vectors[i] = vector;
        }, () =>
        {
            _itemIds.Add(itemId);
            _vectors.Add(vector);
        });
    }

    public void Remove(string itemId)
    {
        Manipulate(itemId, i =>
        {
            _itemIds.RemoveAt(i);
            _vectors.RemoveAt(i);
        }, null);
    }

    private void Manipulate(string itemId, Action<int>? exists, Action? notExists)
    {
        _lock.EnterWriteLock();
        try
        {
            var i = _itemIds.IndexOf(itemId);
            if (i == -1)
            {
                notExists?.Invoke();
            }
            else
            {
                exists?.Invoke(i);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public IList<VectorSearchResult> Search(float[] query, int limit)
    {
        _lock.EnterReadLock();
        try
        {
            var queue = new PriorityQueue<int, float>();
            for (var i = 0; i < limit; i++)
            {
                queue.Enqueue(limit * -1 + i, float.MinValue);
            }

            for (var i = 0; i < _vectors.Count; i++)
            {
                var distance = TensorPrimitives.CosineSimilarity(query, _vectors[i]);
                queue.EnqueueDequeue(i, distance);
            }

            var results = queue.UnorderedItems
                .Where(x => x.Element >= 0)
                .Select(x => new VectorSearchResult
                {
                    ItemId = _itemIds[x.Element],
                    Distance = 1 - x.Priority
                })
                .ToList();

            return results;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}

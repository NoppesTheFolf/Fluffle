using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Noppes.Fluffle.Utils;

public class TopNList<T> : IEnumerable<T>
{
    private readonly int _capacity;
    private readonly IComparer<T> _comparer;
    private readonly PriorityQueue<T, T> _queue;

    public TopNList(int capacity, IComparer<T> comparer)
    {
        _capacity = capacity;
        _comparer = comparer;
        _queue = new PriorityQueue<T, T>(capacity + 1, new ReverseComparer<T>(comparer));
    }

    public void Add(T item)
    {
        if (_queue.Count == _capacity)
        {
            var smallestItem = _queue.Peek();
            var comparisonResult = _comparer.Compare(item, smallestItem);
            if (comparisonResult > 0)
                return;
        }

        _queue.Enqueue(item, item);

        if (_queue.Count > _capacity)
            _queue.Dequeue();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<T> GetEnumerator() => _queue.UnorderedItems.Select(x => x.Element).Order(_comparer).GetEnumerator();
}

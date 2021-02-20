using System;
using System.Buffers;
using System.Collections.Generic;

namespace Noppes.Fluffle.Utils
{
    /// <summary>
    /// A collection which holds a range of integers. Values can be associated to these integers.
    /// After x number of values are associated with an integer, newly added values to said integer
    /// get discarded.
    /// </summary>
    public class DiscardingCollection<T> : IDisposable
    {
        private readonly T[] _buffer;

        private readonly int _length, _sizePerCluster;
        private readonly DiscardingCollectionItem<T>[] _memories;

        public DiscardingCollection(int length, int sizePerCluster)
        {
            if (length < 1)
                throw new ArgumentOutOfRangeException(nameof(length));

            _sizePerCluster = sizePerCluster;

            _length = length;
            _buffer = ArrayPool<T>.Shared.Rent(_length * sizePerCluster);
            var bufferAsMemory = _buffer.AsMemory();

            _memories = ArrayPool<DiscardingCollectionItem<T>>.Shared.Rent(_length);
            for (var i = 0; i < _length; i++)
            {
                _memories[i] ??= new DiscardingCollectionItem<T>();
                _memories[i].Data = bufferAsMemory.Slice(i * sizePerCluster, sizePerCluster);
                _memories[i].Index = 0;
            }
        }

        /// <summary>
        /// Take N number of items from this collection ascendingly.
        /// </summary>
        public IEnumerable<T> Take(int n)
        {
            var currentCount = 0;
            for (var i = 0; i < _length; i++)
            {
                var item = _memories[i];

                for (var k = 0; k < item.Index; k++)
                {
                    yield return item.Data.Span[k];
                    currentCount++;

                    if (currentCount == n)
                        yield break;
                }
            }
        }

        /// <summary>
        /// Add a value to the collection at the given index.
        /// </summary>
        public void Add(ulong index, T value)
        {
            var mem = _memories[index];

            if (mem.Index == _sizePerCluster)
                return;

            mem.Data.Span[mem.Index] = value;
            mem.Index++;
        }

        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(_buffer);
            ArrayPool<DiscardingCollectionItem<T>>.Shared.Return(_memories);
        }
    }
}

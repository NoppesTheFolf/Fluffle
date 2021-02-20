using System;
using System.Collections.Generic;

namespace Noppes.Fluffle.Utils
{
    /// <summary>
    /// A collection which holds <see cref="Memory{T}"/> instances. 
    /// </summary>
    public class MemoryCollection<T>
    {
        /// <summary>
        /// Holds the length of all the <see cref="Memory{T}"/> instances contained by this collection.
        /// </summary>
        public int Length { get; private set; }

        private readonly IList<Memory<T>> _memories;

        public MemoryCollection(IEnumerable<Memory<T>> memories) : this()
        {
            foreach (var memory in memories)
                Add(memory);
        }

        public MemoryCollection()
        {
            _memories = new List<Memory<T>>();
            Length = 0;
        }

        /// <summary>
        /// Add a <see cref="Memory{T}"/> instance to this collection.
        /// </summary>
        /// <param name="memory"></param>
        public void Add(Memory<T> memory)
        {
            _memories.Add(memory);
            Length += memory.Length;
        }

        /// <summary>
        /// Slice the data contained in this memory collection into N number of pieces.
        /// </summary>
        public IEnumerable<List<Memory<T>>> Batch(int n)
        {
            if (n < 1)
                throw new ArgumentOutOfRangeException(nameof(n));

            var valuesPerBucketCount = Length / n;
            var valuesLeftOverCount = Length % n;

            var memoryOffset = 0;
            var memoryIndex = 0;
            var memory = _memories[memoryIndex];
            for (var i = 0; i < n; i++)
            {
                var memoryBucket = new List<Memory<T>>();

                var leftToTake = i != n - 1
                    ? valuesPerBucketCount
                    : valuesPerBucketCount + valuesLeftOverCount;

                while (leftToTake != 0)
                {
                    var takeable = memory.Length - memoryOffset;

                    // We have exhausted our memory
                    if (takeable == 0)
                    {
                        memoryIndex++;
                        memory = _memories[memoryIndex];
                        memoryOffset = 0;
                        continue;
                    }

                    // We can take more than we have to
                    if (leftToTake <= takeable)
                    {
                        memoryBucket.Add(memory.Slice(memoryOffset, leftToTake));
                        memoryOffset += leftToTake;
                        leftToTake = 0;
                        continue;
                    }

                    // There is less available than we need
                    memoryBucket.Add(memory.Slice(memoryOffset, takeable));
                    leftToTake -= takeable;
                    memoryOffset = memory.Length;
                }

                yield return memoryBucket;
            }
        }
    }
}

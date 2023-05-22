using System;
using System.Collections.Generic;
using System.Linq;

namespace Noppes.Fluffle.Utils;

public static class AsyncEnumerableExtensions
{
    /// <summary>
    /// Some simple code to create a batch out an <see cref="IAsyncEnumerable{T}"/> because
    /// sometimes it's better to write some custom code rather than install a library.
    /// </summary>
    public static async IAsyncEnumerable<ICollection<T>> Batch<T>(this IAsyncEnumerable<T> values, int size)
    {
        if (size < 1)
            throw new ArgumentOutOfRangeException(nameof(size), size, "Batch size cannot be less than 1.");

        var batch = new List<T>();
        await foreach (var value in values)
        {
            batch.Add(value);

            if (batch.Count != size)
                continue;

            yield return batch;
            batch = new List<T>();
        }

        if (batch.Any())
            yield return batch;
    }
}

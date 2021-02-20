using System;

namespace Noppes.Fluffle.Utils
{
    /// <summary>
    /// Internally used class to support the <see cref="DiscardingCollection{T}"/> class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DiscardingCollectionItem<T>
    {
        public Memory<T> Data { get; set; }

        public int Index { get; set; }
    }
}

using System.Collections.Generic;

namespace Noppes.Fluffle.Main.Communication
{
    public interface ITrackableModel<T>
    {
        public long NextChangeId { get; set; }

        public IEnumerable<T> Results { get; set; }
    }
}

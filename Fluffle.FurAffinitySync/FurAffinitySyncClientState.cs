using Noppes.Fluffle.Sync;
using System.Collections.Generic;

namespace Noppes.Fluffle.FurAffinitySync
{
    public class FurAffinitySyncClientState : SyncState
    {
        public int ArchiveStartId { get; set; }

        public int ArchiveEndId { get; set; }

        public ICollection<string> ProcessedArtists { get; set; }

        public FurAffinitySyncClientState()
        {
            ProcessedArtists = new List<string>();
        }
    }
}

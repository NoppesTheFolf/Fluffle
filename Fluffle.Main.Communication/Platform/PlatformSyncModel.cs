using Noppes.Fluffle.Constants;
using System;
using System.Collections.Generic;

namespace Noppes.Fluffle.Main.Communication
{
    public class PlatformSyncModel
    {
        public class SyncInfo
        {
            public SyncTypeConstant Type { get; set; }

            public DateTime When { get; set; }

            public TimeSpan TimeToWait { get; set; }
        }

        public SyncInfo Next { get; set; }

        public ICollection<SyncInfo> Other { get; set; }
    }
}

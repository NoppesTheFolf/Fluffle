using System;
using System.Collections.Generic;

namespace Noppes.Fluffle.Main.Communication
{
    public class StatusModelHistory
    {
        public DateTimeOffset When { get; set; }

        public int ScrapedCount { get; set; }

        public int IndexedCount { get; set; }

        public int ErrorCount { get; set; }
    }

    public class StatusModel
    {
        public string Name { get; set; }

        public int EstimatedCount { get; set; }

        public int StoredCount { get; set; }

        public int IndexedCount { get; set; }

        public bool IsComplete { get; set; }

        public IEnumerable<StatusModelHistory> HistoryLast30Days { get; set; }

        public IEnumerable<StatusModelHistory> HistoryLast24Hours { get; set; }
    }
}

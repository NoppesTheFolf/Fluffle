namespace Noppes.Fluffle.Main.Communication
{
    public class StatusModel
    {
        public string Name { get; set; }

        public int EstimatedCount { get; set; }

        public int StoredCount { get; set; }

        public int IndexedCount { get; set; }

        public bool IsComplete { get; set; }
    }
}

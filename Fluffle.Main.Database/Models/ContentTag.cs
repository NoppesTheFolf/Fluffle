using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.Main.Database.Models
{
    public class ContentTag : BaseEntity
    {
        public int ContentId { get; set; }
        public int TagId { get; set; }

        public virtual Content Content { get; set; }
        public virtual Tag Tag { get; set; }
    }
}

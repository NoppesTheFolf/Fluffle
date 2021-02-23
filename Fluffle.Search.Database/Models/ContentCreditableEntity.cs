using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.Search.Database.Models
{
    public partial class ContentCreditableEntity : BaseEntity
    {
        public int ContentId { get; set; }
        public int CreditableEntityId { get; set; }

        public virtual Content Content { get; set; }
        public virtual CreditableEntity CreditableEntity { get; set; }
    }
}

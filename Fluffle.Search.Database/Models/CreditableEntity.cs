using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Database;
using System.Collections.Generic;

namespace Noppes.Fluffle.Search.Database.Models
{
    public partial class CreditableEntity : BaseEntity, IConfigurable<CreditableEntity>, ITrackable
    {
        public CreditableEntity()
        {
            Content = new HashSet<Content>();
            ContentCreditableEntity = new HashSet<ContentCreditableEntity>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public long ChangeId { get; set; }

        public CreditableEntityType Type { get; set; }

        public virtual ICollection<Content> Content { get; set; }
        public virtual ICollection<ContentCreditableEntity> ContentCreditableEntity { get; set; }

        public void Configure(EntityTypeBuilder<CreditableEntity> entity)
        {
            entity.Property(e => e.Id);
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired();

            entity.Property(e => e.ChangeId);
            entity.HasIndex(e => e.ChangeId).IsUnique();

            entity.Property(e => e.Type);
        }
    }
}

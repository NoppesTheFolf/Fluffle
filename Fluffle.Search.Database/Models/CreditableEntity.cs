using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.Search.Database.Models;

public partial class CreditableEntity : BaseEntity, IConfigurable<CreditableEntity>, ITrackable
{
    public int Id { get; set; }

    public int PlatformId { get; set; }

    public string Name { get; set; }

    public long ChangeId { get; set; }

    public CreditableEntityType Type { get; set; }

    public int? Priority { get; set; }

    public virtual Platform Platform { get; set; }

    public void Configure(EntityTypeBuilder<CreditableEntity> entity)
    {
        entity.Property(e => e.Id);
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Name).IsRequired();

        entity.Property(e => e.ChangeId);
        entity.HasIndex(e => new { e.PlatformId, e.ChangeId });

        entity.Property(e => e.Type);

        entity.Property(e => e.Priority);

        entity.HasOne(d => d.Platform)
            .WithMany(p => p.CreditableEntities)
            .HasForeignKey(d => d.PlatformId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

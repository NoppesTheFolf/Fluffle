using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.Main.Database.Models;

public partial class ContentWarning : TrackedBaseEntity, IConfigurable<ContentWarning>
{
    public int Id { get; set; }

    public int ContentId { get; set; }

    public string Message { get; set; }

    public virtual Content Content { get; set; }

    public void Configure(EntityTypeBuilder<ContentWarning> entity)
    {
        entity.Property(e => e.Id);
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Message).IsRequired();

        entity.Property(e => e.ContentId);
        entity.HasOne(d => d.Content)
            .WithMany(p => p.Warnings)
            .HasForeignKey(d => d.ContentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

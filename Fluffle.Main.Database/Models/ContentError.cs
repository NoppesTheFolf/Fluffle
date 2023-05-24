using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.Main.Database.Models;

public partial class ContentError : TrackedBaseEntity, IConfigurable<ContentError>
{
    public int Id { get; set; }

    public string Message { get; set; }

    public bool IsFatal { get; set; }

    public int ContentId { get; set; }

    public virtual Content Content { get; set; }

    public void Configure(EntityTypeBuilder<ContentError> entity)
    {
        entity.Property(e => e.Id);
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Message).IsRequired();

        entity.Property(e => e.IsFatal);

        // Speeds up calculating error history in index service
        entity.HasIndex(e => e.CreatedAt);

        entity.Property(e => e.ContentId);
        entity.HasOne(d => d.Content)
            .WithMany(p => p.Errors)
            .HasForeignKey(d => d.ContentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

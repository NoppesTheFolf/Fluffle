using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Models;

public class ContentOtherSource : BaseEntity, IConfigurable<ContentOtherSource>
{
    public int Id { get; set; }

    public string Location { get; set; }

    public int ContentId { get; set; }
    public virtual Content Content { get; set; }

    public void Configure(EntityTypeBuilder<ContentOtherSource> entity)
    {
        entity.Property(e => e.Id);
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Location).HasMaxLength(2048).IsRequired();

        entity.Property(e => e.ContentId);
        entity.HasOne(e => e.Content)
            .WithMany(e => e.OtherSources)
            .HasForeignKey(e => e.ContentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.Main.Database.Models;

public partial class ContentFile : BaseEntity, IConfigurable<ContentFile>
{
    public int ContentId { get; set; }
    public int FileFormatId { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Location { get; set; }

    public virtual Content Content { get; set; }
    public virtual FileFormat Format { get; set; }

    public void Configure(EntityTypeBuilder<ContentFile> entity)
    {
        entity.HasKey(e => new { e.ContentId, e.Location });

        entity.Property(e => e.Width);
        entity.Property(e => e.Height);

        entity.Property(e => e.Location)
            .IsRequired()
            .HasMaxLength(2048);

        entity.Property(e => e.ContentId);
        entity.HasOne(d => d.Content)
            .WithMany(p => p.Files)
            .HasForeignKey(d => d.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.Property(e => e.FileFormatId);
        entity.Property(e => e.FileFormatId);
        entity.HasOne(d => d.Format)
            .WithMany(p => p.ContentFiles)
            .HasForeignKey(d => d.FileFormatId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

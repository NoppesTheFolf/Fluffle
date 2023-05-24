using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.Search.Database.Models;

public class ImageHash : BaseEntity, IConfigurable<ImageHash>
{
    public int Id { get; set; }

    public byte[] PhashRed64 { get; set; }
    public byte[] PhashGreen64 { get; set; }
    public byte[] PhashBlue64 { get; set; }
    public byte[] PhashAverage64 { get; set; }

    public byte[] PhashRed256 { get; set; }
    public byte[] PhashGreen256 { get; set; }
    public byte[] PhashBlue256 { get; set; }
    public byte[] PhashAverage256 { get; set; }

    public byte[] PhashRed1024 { get; set; }
    public byte[] PhashGreen1024 { get; set; }
    public byte[] PhashBlue1024 { get; set; }
    public byte[] PhashAverage1024 { get; set; }

    public virtual Image Image { get; set; }

    public void Configure(EntityTypeBuilder<ImageHash> entity)
    {
        entity.Property(e => e.Id)
            .ValueGeneratedNever();

        entity.HasKey(e => e.Id);

        entity.HasOne(d => d.Image)
            .WithOne(p => p.ImageHash)
            .HasForeignKey<ImageHash>(d => d.Id);

        entity.Property(e => e.PhashRed64).IsRequired();
        entity.Property(e => e.PhashGreen64).IsRequired();
        entity.Property(e => e.PhashBlue64).IsRequired();
        entity.Property(e => e.PhashAverage64).IsRequired();

        entity.Property(e => e.PhashRed256).IsRequired();
        entity.Property(e => e.PhashGreen256).IsRequired();
        entity.Property(e => e.PhashBlue256).IsRequired();
        entity.Property(e => e.PhashAverage256).IsRequired();

        entity.Property(e => e.PhashRed1024).IsRequired();
        entity.Property(e => e.PhashGreen1024).IsRequired();
        entity.Property(e => e.PhashBlue1024).IsRequired();
        entity.Property(e => e.PhashAverage1024).IsRequired();
    }
}

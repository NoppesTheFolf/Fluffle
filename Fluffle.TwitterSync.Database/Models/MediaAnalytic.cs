using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.TwitterSync.Database.Models;

public class MediaAnalytic : BaseEntity, IConfigurable<MediaAnalytic>
{
    public string Id { get; set; }
    public virtual Media Media { get; set; }

    public double True { get; set; }

    public double False { get; set; }

    public void Configure(EntityTypeBuilder<MediaAnalytic> entity)
    {
        entity.Property(e => e.Id).HasMaxLength(20).ValueGeneratedNever();
        entity.HasKey(e => e.Id);

        entity.Property(e => e.True);
        entity.Property(e => e.False);

        entity.HasOne(e => e.Media)
            .WithOne(e => e.MediaAnalytic)
            .HasForeignKey<MediaAnalytic>(e => e.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

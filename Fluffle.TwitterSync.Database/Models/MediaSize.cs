using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.TwitterSync.Database.Models;

public enum MediaSizeConstant
{
    Thumb = 0,
    Small = 1,
    Medium = 2,
    Large = 3,
}

public enum ResizeMode
{
    Crop = 0,
    Fit = 1
}

public class MediaSize : BaseEntity, IConfigurable<MediaSize>
{
    public string MediaId { get; set; }
    public virtual Media Media { get; set; }

    public MediaSizeConstant Size { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public ResizeMode ResizeMode { get; set; }

    public void Configure(EntityTypeBuilder<MediaSize> entity)
    {
        entity.Property(e => e.MediaId).HasMaxLength(20);
        entity.Property(e => e.Size);
        entity.HasKey(e => new { e.MediaId, e.Size });

        entity.Property(e => e.Width);
        entity.Property(e => e.Height);
        entity.Property(e => e.ResizeMode);

        entity.HasOne(e => e.Media)
            .WithMany(e => e.Sizes)
            .HasForeignKey(e => e.MediaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

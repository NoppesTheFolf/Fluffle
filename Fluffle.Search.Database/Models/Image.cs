using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Noppes.Fluffle.Search.Database.Models;

public class Image : ITrackable
{
    public int Id { get; set; }

    public int PlatformId { get; set; }

    public string Location { get; set; }

    public bool IsSfw { get; set; }

    public byte[] CompressedImageHashes { get; set; }

    public string ThumbnailLocation { get; set; }

    public int ThumbnailWidth { get; set; }

    public int ThumbnailCenterX { get; set; }

    public int ThumbnailHeight { get; set; }

    public int ThumbnailCenterY { get; set; }

    public int[] Credits { get; set; }

    public long ChangeId { get; set; }

    public bool IsDeleted { get; set; }

    public static void Configure(EntityTypeBuilder<Image> entity)
    {
        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.HasKey(e => e.Id);

        entity.Property(e => e.PlatformId);
        entity.Property(e => e.Location).IsRequired();
        entity.Property(e => e.IsSfw);

        entity.Property(e => e.CompressedImageHashes).IsRequired();

        entity.Property(e => e.ThumbnailLocation).IsRequired();
        entity.Property(e => e.ThumbnailWidth);
        entity.Property(e => e.ThumbnailCenterX);
        entity.Property(e => e.ThumbnailHeight);
        entity.Property(e => e.ThumbnailCenterY);

        entity.Property(e => e.Credits).IsRequired();

        entity.Property(e => e.ChangeId);
        entity.HasIndex(e => new { e.PlatformId, e.ChangeId }).IsUnique();

        entity.Property(e => e.IsDeleted);
    }
}

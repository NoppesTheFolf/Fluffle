using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.Search.Database.Models
{
    public class DenormalizedImage : BaseEntity, IConfigurable<DenormalizedImage>, ITrackable
    {
        public int Id { get; set; }

        public int PlatformId { get; set; }

        public string Location { get; set; }

        public bool IsSfw { get; set; }

        public byte[] PhashAverage64 { get; set; }

        public byte[] PhashRed256 { get; set; }
        public byte[] PhashGreen256 { get; set; }
        public byte[] PhashBlue256 { get; set; }
        public byte[] PhashAverage256 { get; set; }

        public byte[] PhashRed1024 { get; set; }
        public byte[] PhashGreen1024 { get; set; }
        public byte[] PhashBlue1024 { get; set; }
        public byte[] PhashAverage1024 { get; set; }

        public string ThumbnailLocation { get; set; }

        public int ThumbnailWidth { get; set; }

        public int ThumbnailCenterX { get; set; }

        public int ThumbnailHeight { get; set; }

        public int ThumbnailCenterY { get; set; }

        public int[] Credits { get; set; }

        public long ChangeId { get; set; }

        public bool IsDeleted { get; set; }

        public void Configure(EntityTypeBuilder<DenormalizedImage> entity)
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PlatformId);
            entity.Property(e => e.Location).IsRequired();
            entity.Property(e => e.IsSfw);

            entity.Property(e => e.PhashAverage64).IsRequired();

            entity.Property(e => e.PhashRed256).IsRequired();
            entity.Property(e => e.PhashGreen256).IsRequired();
            entity.Property(e => e.PhashBlue256).IsRequired();
            entity.Property(e => e.PhashAverage256).IsRequired();

            entity.Property(e => e.PhashRed1024).IsRequired();
            entity.Property(e => e.PhashGreen1024).IsRequired();
            entity.Property(e => e.PhashBlue1024).IsRequired();
            entity.Property(e => e.PhashAverage1024).IsRequired();

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
}

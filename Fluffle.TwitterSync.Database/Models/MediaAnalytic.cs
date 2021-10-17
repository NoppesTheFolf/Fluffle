using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.TwitterSync.Database.Models
{
    public class MediaAnalytic : BaseEntity, IConfigurable<MediaAnalytic>
    {
        public string Id { get; set; }
        public virtual Media Media { get; set; }

        public double FurryArt { get; set; }

        public double Real { get; set; }

        public double Fursuit { get; set; }

        public double Anime { get; set; }

        public int[] ArtistIds { get; set; }

        public void Configure(EntityTypeBuilder<MediaAnalytic> entity)
        {
            entity.Property(e => e.Id).HasMaxLength(20).ValueGeneratedNever();
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FurryArt);
            entity.Property(e => e.Real);
            entity.Property(e => e.Fursuit);
            entity.Property(e => e.Anime);

            entity.Property(e => e.ArtistIds).IsRequired();

            entity.HasOne(e => e.Media)
                .WithOne(e => e.MediaAnalytic)
                .HasForeignKey<MediaAnalytic>(e => e.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

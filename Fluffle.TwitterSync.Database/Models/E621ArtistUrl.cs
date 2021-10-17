using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.TwitterSync.Database.Models
{
    public class E621ArtistUrl : BaseEntity, IConfigurable<E621ArtistUrl>
    {
        public int Id { get; set; }

        public string TwitterUsername { get; set; }

        public bool? TwitterExists { get; set; }

        public int ArtistId { get; set; }
        public virtual E621Artist Artist { get; set; }

        public void Configure(EntityTypeBuilder<E621ArtistUrl> entity)
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TwitterUsername).HasMaxLength(15).IsRequired();
            entity.Property(e => e.TwitterExists);

            entity.Property(e => e.ArtistId);
            entity.HasOne(e => e.Artist)
                .WithMany(e => e.Urls)
                .HasForeignKey(e => e.ArtistId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

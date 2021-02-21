using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.Main.Database.Models
{
    public partial class IndexStatistic : BaseEntity, IConfigurable<IndexStatistic>
    {
        public int PlatformId { get; set; }
        public int MediaTypeId { get; set; }
        public int Count { get; set; }
        public int IndexedCount { get; set; }

        public virtual Platform Platform { get; set; }
        public virtual MediaType MediaType { get; set; }

        public void Configure(EntityTypeBuilder<IndexStatistic> entity)
        {
            entity.Property(e => e.PlatformId);
            entity.Property(e => e.MediaTypeId);
            entity.HasKey(e => new { e.PlatformId, e.MediaTypeId });

            entity.HasOne(e => e.Platform)
                .WithMany(e => e.IndexStatistics)
                .HasForeignKey(e => e.PlatformId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.MediaType)
                .WithMany(e => e.IndexStatistics)
                .HasForeignKey(e => e.MediaTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Count);
            entity.Property(e => e.IndexedCount);
        }
    }
}

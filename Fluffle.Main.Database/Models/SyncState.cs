using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.Main.Database.Models
{
    public class SyncState : TrackedBaseEntity, IConfigurable<SyncState>
    {
        public int Id { get; set; }
        public string Document { get; set; }
        public int Version { get; set; }

        public virtual Platform Platform { get; set; }

        public void Configure(EntityTypeBuilder<SyncState> entity)
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Document).IsRequired();

            entity.Property(e => e.Version);

            entity.HasOne(d => d.Platform)
                .WithOne(p => p.SyncState)
                .HasForeignKey<SyncState>(d => d.Id);
        }
    }
}

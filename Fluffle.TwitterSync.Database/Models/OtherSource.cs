using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.TwitterSync.Database.Models
{
    public class OtherSource : BaseEntity, IConfigurable<OtherSource>
    {
        public int Id { get; set; }

        public string Location { get; set; }

        public bool HasBeenProcessed { get; set; }

        public void Configure(EntityTypeBuilder<OtherSource> entity)
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Location).IsRequired();

            entity.Property(e => e.HasBeenProcessed);
        }
    }
}

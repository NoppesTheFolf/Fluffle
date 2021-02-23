using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.Search.Database.Models
{
    public partial class ContentFile : BaseEntity, IConfigurable<ContentFile>
    {
        public int ContentId { get; set; }
        public FileFormatConstant Format { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Location { get; set; }

        public virtual Content Content { get; set; }

        public void Configure(EntityTypeBuilder<ContentFile> entity)
        {
            entity.HasKey(e => new { e.ContentId, e.Location });

            entity.Property(e => e.Width);
            entity.Property(e => e.Height);

            entity.Property(e => e.Location)
                .IsRequired()
                .HasMaxLength(2048);

            entity.Property(e => e.ContentId);
            entity.HasOne(d => d.Content)
                .WithMany(p => p.Files)
                .HasForeignKey(d => d.ContentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

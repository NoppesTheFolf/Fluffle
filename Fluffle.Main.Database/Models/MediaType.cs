using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;
using System.Collections.Generic;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Models
{
    public partial class MediaType : BaseEntity, IConfigurable<MediaType>
    {
        public MediaType()
        {
            FileFormats = new HashSet<FileFormat>();
            Content = new HashSet<Content>();
        }

        public int Id { get; set; }
        [Sync] public string Name { get; set; }

        public virtual ICollection<FileFormat> FileFormats { get; set; }
        public virtual ICollection<Content> Content { get; set; }

        public void Configure(EntityTypeBuilder<MediaType> entity)
        {
            entity.Property(e => e.Id)
                .ValueGeneratedNever();
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(32);
            entity.HasIndex(e => e.Name)
                .IsUnique();
        }
    }
}

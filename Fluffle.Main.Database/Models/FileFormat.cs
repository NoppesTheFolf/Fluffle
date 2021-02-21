using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;
using System.Collections.Generic;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Models
{
    public partial class FileFormat : BaseEntity, IConfigurable<FileFormat>
    {
        public FileFormat()
        {
            ContentFiles = new List<ContentFile>();
        }

        public int Id { get; set; }
        [Sync] public string Name { get; set; }
        [Sync] public string Abbreviation { get; set; }
        [Sync] public string Extension { get; set; }

        public virtual ICollection<ContentFile> ContentFiles { get; set; }

        public void Configure(EntityTypeBuilder<FileFormat> entity)
        {
            entity.Property(e => e.Id);
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(32);

            entity.HasIndex(e => e.Name)
                .IsUnique();

            entity.Property(e => e.Abbreviation)
                .IsRequired()
                .HasMaxLength(8);

            entity.HasIndex(e => e.Abbreviation)
                .IsUnique();

            entity.Property(e => e.Extension)
                .IsRequired()
                .HasMaxLength(4);

            entity.HasIndex(e => e.Extension)
                .IsUnique();
        }
    }
}

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;
using System.Collections.Generic;

namespace Noppes.Fluffle.Main.Database.Models
{
    public class Tag : BaseEntity, IConfigurable<Tag>
    {
        public Tag()
        {
            Content = new HashSet<Content>();
            ContentTags = new HashSet<ContentTag>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Content> Content { get; set; }
        public virtual ICollection<ContentTag> ContentTags { get; set; }

        public void Configure(EntityTypeBuilder<Tag> entity)
        {
            entity.Property(e => e.Id);
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        }
    }
}

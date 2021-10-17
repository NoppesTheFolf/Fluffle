using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;
using System.Collections.Generic;

namespace Noppes.Fluffle.TwitterSync.Database.Models
{
    public class E621Artist : BaseEntity, IConfigurable<E621Artist>
    {
        public E621Artist()
        {
            Urls = new HashSet<E621ArtistUrl>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<E621ArtistUrl> Urls { get; set; }

        public void Configure(EntityTypeBuilder<E621Artist> entity)
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired();
        }
    }
}

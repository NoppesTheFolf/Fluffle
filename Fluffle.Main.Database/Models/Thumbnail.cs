using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;
using System.Collections.Generic;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Models;

public partial class Thumbnail : TrackedBaseEntity, IConfigurable<Thumbnail>
{
    public Thumbnail()
    {
        Content = new HashSet<Content>();
    }

    public int Id { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int CenterX { get; set; }
    public int CenterY { get; set; }
    public string Location { get; set; }

    public string Filename { get; set; }
    public string B2FileId { get; set; }

    public virtual ICollection<Content> Content { get; set; }

    public void Configure(EntityTypeBuilder<Thumbnail> entity)
    {
        entity.Property(e => e.Id);
        entity.HasKey(e => e.Id);

        entity.Property(e => e.B2FileId).IsRequired();
        entity.HasIndex(e => e.B2FileId);
        entity.Property(e => e.Filename).IsRequired();
        entity.HasIndex(e => e.Filename);

        entity.Property(e => e.Width);
        entity.Property(e => e.Height);
        entity.Property(e => e.CenterX);
        entity.Property(e => e.CenterY);
        entity.Property(e => e.Location).IsRequired();
    }
}

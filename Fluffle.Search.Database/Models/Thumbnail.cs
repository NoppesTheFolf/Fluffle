using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;
using System.Collections.Generic;

namespace Noppes.Fluffle.Search.Database.Models;

public partial class Thumbnail : BaseEntity, IConfigurable<Thumbnail>
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

    public ICollection<Content> Content { get; set; }

    public void Configure(EntityTypeBuilder<Thumbnail> entity)
    {
        entity.Property(e => e.Id);
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Width);
        entity.Property(e => e.Height);
        entity.Property(e => e.CenterX);
        entity.Property(e => e.CenterY);
        entity.Property(e => e.Location).IsRequired();
    }
}

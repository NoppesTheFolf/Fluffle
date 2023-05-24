using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.Database.Models;
using System.Collections.Generic;

namespace Noppes.Fluffle.Search.Database.Models;

public partial class Platform : BaseEntity, IConfigurable<Platform>, IPlatform
{
    public Platform()
    {
        Content = new HashSet<Content>();
        CreditableEntities = new HashSet<CreditableEntity>();
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string NormalizedName { get; set; }

    public virtual ICollection<Content> Content { get; set; }
    public virtual ICollection<CreditableEntity> CreditableEntities { get; set; }

    public void Configure(EntityTypeBuilder<Platform> entity)
    {
        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Name).IsRequired()
            .HasMaxLength(32);
        entity.HasIndex(e => e.Name)
            .IsUnique();

        entity.Property(e => e.NormalizedName).IsRequired()
            .HasMaxLength(40);
        entity.HasIndex(e => e.NormalizedName)
            .IsUnique();
    }
}

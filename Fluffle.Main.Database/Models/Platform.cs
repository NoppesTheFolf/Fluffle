using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.Database.Models;
using System.Collections.Generic;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Models;

public partial class Platform : BaseEntity, IConfigurable<Platform>, IPlatform
{
    public Platform()
    {
        Content = new HashSet<Content>();
        PlatformSyncs = new HashSet<PlatformSync>();
        CreditableEntities = new HashSet<CreditableEntity>();
    }

    public int Id { get; set; }
    public bool IsComplete { get; set; }
    [Sync] public string Name { get; set; }
    [Sync] public string NormalizedName { get; set; }
    [Sync] public int EstimatedContentCount { get; set; }
    [Sync] public string HomeLocation { get; set; }

    public virtual SyncState SyncState { get; set; }

    public virtual ICollection<Content> Content { get; set; }
    public virtual ICollection<PlatformSync> PlatformSyncs { get; set; }
    public virtual ICollection<CreditableEntity> CreditableEntities { get; set; }

    public void Configure(EntityTypeBuilder<Platform> entity)
    {
        entity.Property(e => e.Id)
            .ValueGeneratedNever();
        entity.HasKey(e => e.Id);

        entity.Property(e => e.IsComplete);

        entity.Property(e => e.Name).IsRequired()
            .HasMaxLength(32);
        entity.HasIndex(e => e.Name)
            .IsUnique();

        entity.Property(e => e.NormalizedName).IsRequired()
            .HasMaxLength(40);
        entity.HasIndex(e => e.NormalizedName)
            .IsUnique();

        entity.Property(e => e.EstimatedContentCount);

        entity.Property(e => e.HomeLocation)
            .IsRequired()
            .HasMaxLength(64);
    }
}

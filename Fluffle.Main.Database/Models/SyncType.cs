using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;
using System.Collections.Generic;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Models;

public partial class SyncType : BaseEntity, IConfigurable<SyncType>
{
    public SyncType()
    {
        PlatformSyncs = new HashSet<PlatformSync>();
    }

    public int Id { get; set; }
    [Sync] public string Name { get; set; }

    public virtual ICollection<PlatformSync> PlatformSyncs { get; set; }

    public void Configure(EntityTypeBuilder<SyncType> entity)
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

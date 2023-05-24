using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;
using System;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Models;

public partial class PlatformSync : BaseEntity, IConfigurable<PlatformSync>
{
    public int PlatformId { get; set; }
    public int SyncTypeId { get; set; }
    public TimeSpan Interval { get; set; }
    public DateTime When { get; set; }

    public virtual Platform Platform { get; set; }
    public virtual SyncType SyncType { get; set; }

    public void Configure(EntityTypeBuilder<PlatformSync> entity)
    {
        entity.Property(e => e.PlatformId);
        entity.Property(e => e.SyncTypeId);
        entity.HasKey(e => new { e.PlatformId, e.SyncTypeId });

        entity.HasOne(d => d.Platform)
            .WithMany(p => p.PlatformSyncs)
            .HasForeignKey(d => d.PlatformId);

        entity.HasOne(d => d.SyncType)
            .WithMany(p => p.PlatformSyncs)
            .HasForeignKey(d => d.SyncTypeId);

        entity.Property(e => e.Interval);

        entity.Property(e => e.When)
            .HasColumnType("timestamp with time zone");
    }
}

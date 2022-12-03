using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;
using System;

namespace Noppes.Fluffle.DeviantArt.Database.Entities;

public class Deviation : TrackedBaseEntity, IConfigurable<Deviation>
{
    public string Id { get; set; }

    public string Location { get; set; }

    public string Title { get; set; }

    public string[] Tags { get; set; }

    public string DeviantId { get; set; }
    public virtual Deviant Deviant { get; set; }

    public DateTime ProcessedAt { get; set; }

    public void Configure(EntityTypeBuilder<Deviation> entity)
    {
        entity.Property(x => x.Id).IsRequired();
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Location).IsRequired();

        entity.Property(x => x.Title).IsRequired();

        entity.Property(x => x.Tags).IsRequired();

        entity.Property(x => x.DeviantId).IsRequired();
        entity.HasOne(x => x.Deviant)
            .WithMany(x => x.Deviations)
            .HasForeignKey(x => x.DeviantId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.Property(x => x.ProcessedAt);
    }
}
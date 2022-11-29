using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;
using System;
using System.Collections.Generic;

namespace Noppes.Fluffle.DeviantArt.Database.Entities;

public class Deviant : TrackedBaseEntity, IConfigurable<Deviant>
{
    public Deviant()
    {
        Deviations = new HashSet<Deviation>();
    }

    public string Id { get; set; }

    public string Username { get; set; }

    public string IconLocation { get; set; }

    public DateTime JoinedWhen { get; set; }

    public bool? IsFurryArtist { get; set; }

    public DateTime? IsFurryArtistEnqueuedWhen { get; set; }

    public DateTime? IsFurryArtistDeterminedWhen { get; set; }

    public DateTime? GalleryScrapedWhen { get; set; }

    public virtual ICollection<Deviation> Deviations { get; set; }

    public void Configure(EntityTypeBuilder<Deviant> entity)
    {
        entity.Property(x => x.Id).IsRequired();
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Username).IsRequired();

        entity.Property(x => x.IconLocation).IsRequired();

        entity.Property(x => x.JoinedWhen);

        entity.Property(x => x.IsFurryArtist);
        entity.HasIndex(x => x.IsFurryArtist);

        entity.Property(x => x.IsFurryArtistEnqueuedWhen);

        entity.Property(x => x.IsFurryArtistDeterminedWhen);

        entity.Property(x => x.GalleryScrapedWhen);
    }
}
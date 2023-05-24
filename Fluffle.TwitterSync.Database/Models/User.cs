using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;
using System;
using System.Collections.Generic;

namespace Noppes.Fluffle.TwitterSync.Database.Models;

public class User : BaseEntity, IConfigurable<User>
{
    public User()
    {
        Tweets = new HashSet<Tweet>();
    }

    public string Id { get; set; }

    public string Name { get; set; }

    public string Username { get; set; }

    public bool IsProtected { get; set; }

    public bool IsSuspended { get; set; }

    public int FollowersCount { get; set; }

    public bool IsOnE621 { get; set; }

    public bool? IsFurryArtist { get; set; }

    public long ReservedUntil { get; set; }

    public DateTimeOffset? TimelineRetrievedAt { get; set; }

    public DateTimeOffset? TimelineNextRetrievalAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Tweet> Tweets { get; set; }

    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.Property(e => e.Id).HasMaxLength(20).ValueGeneratedNever();
        entity.HasKey(e => e.Id);

        // Twitter counts emojis and such as two characters, although in UTF-8 they would be
        // considered to have a length of 4.
        entity.Property(e => e.Name).HasMaxLength(50 * 2).IsRequired();
        entity.Property(e => e.Username).HasMaxLength(15).IsRequired();

        entity.Property(e => e.FollowersCount);

        entity.Property(e => e.IsProtected);
        entity.Property(e => e.IsSuspended);

        entity.Property(e => e.IsOnE621);
        entity.Property(e => e.IsFurryArtist);

        entity.Property(e => e.ReservedUntil);
        entity.Property(e => e.TimelineRetrievedAt);
        entity.Property(e => e.TimelineNextRetrievalAt);

        entity.Property(e => e.IsDeleted);

        // Speeds up supplying new users
        entity.HasIndex(e => new { e.IsFurryArtist, e.IsOnE621, e.IsProtected, e.IsSuspended, e.IsDeleted, e.ReservedUntil, e.FollowersCount });

        // Speed up supplying existing users
        entity.HasIndex(e => new { e.IsFurryArtist, e.IsProtected, e.IsSuspended, e.IsDeleted, e.ReservedUntil, e.TimelineNextRetrievalAt });
    }
}

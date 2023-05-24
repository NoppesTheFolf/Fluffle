using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Database;
using System;

namespace Noppes.Fluffle.TwitterSync.Database.Models;

public class TwitterDesignTimeContext : DesignTimeContext<TwitterContext>
{
}

public class TwitterContext : BaseContext
{
    public override Type ConfigurationType => typeof(TwitterDatabaseConfiguration);

    public TwitterContext()
    {
    }

    public TwitterContext(DbContextOptions options) : base(options)
    {
    }

    public virtual DbSet<E621Artist> E621Artists { get; set; }
    public virtual DbSet<E621ArtistUrl> E621ArtistUrls { get; set; }
    public virtual DbSet<OtherSource> OtherSources { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Tweet> Tweets { get; set; }
    public virtual DbSet<TweetMedia> TweetMedia { get; set; }
    public virtual DbSet<Media> Media { get; set; }
    public virtual DbSet<MediaSize> MediaSizes { get; set; }
    public virtual DbSet<MediaAnalytic> MediaAnalytics { get; set; }
}

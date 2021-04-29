using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Api.AccessControl;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Database;
using System;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Models
{
    public class DesignTimeContext : DesignTimeContext<FluffleContext>
    {
    }

    public partial class FluffleContext : ApiKeyContext<ApiKey, Permission, ApiKeyPermission>
    {
        public override Type ConfigurationType => typeof(MainDatabaseConfiguration);

        public FluffleContext()
        {
        }

        public FluffleContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FaPopularArtist>(e =>
            {
                e.HasNoKey();
                e.ToView("fa_popular_artists");
                e.Property(e => e.ArtistId).HasColumnName("artist_id");
                e.Property(e => e.AverageScore).HasColumnName("average_score");
            });

            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<FaPopularArtist> FaPopularArtists { get; set; }
        public virtual DbSet<CreditableEntity> CreditableEntities { get; set; }
        public virtual DbSet<ContentCreditableEntity> ContentCreditableEntities { get; set; }
        public virtual DbSet<Content> Content { get; set; }
        public virtual DbSet<ContentFile> ContentFiles { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<FileFormat> FileFormats { get; set; }
        public virtual DbSet<ImageHash> ImageHashes { get; set; }
        public virtual DbSet<ContentRating> ImageRatings { get; set; }
        public virtual DbSet<MediaType> MediaTypes { get; set; }
        public virtual DbSet<Platform> Platforms { get; set; }
        public virtual DbSet<SyncState> SyncStates { get; set; }
        public virtual DbSet<PlatformSync> PlatformSyncs { get; set; }
        public virtual DbSet<SyncType> SyncTypes { get; set; }
        public virtual DbSet<Thumbnail> Thumbnails { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<ContentTag> ContentTags { get; set; }
    }
}

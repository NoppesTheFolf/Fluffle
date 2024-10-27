using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Api.AccessControl;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Database;
using System;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Models;

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

    public virtual DbSet<CreditableEntity> CreditableEntities { get; set; }
    public virtual DbSet<ContentCreditableEntity> ContentCreditableEntities { get; set; }
    public virtual DbSet<Content> Content { get; set; }
    public virtual DbSet<ContentFile> ContentFiles { get; set; }
    public virtual DbSet<Image> Images { get; set; }
    public virtual DbSet<FileFormat> FileFormats { get; set; }
    public virtual DbSet<ImageHash> ImageHashes { get; set; }
    public virtual DbSet<ContentRating> ContentRatings { get; set; }
    public virtual DbSet<MediaType> MediaTypes { get; set; }
    public virtual DbSet<Platform> Platforms { get; set; }
    public virtual DbSet<SyncState> SyncStates { get; set; }
    public virtual DbSet<PlatformSync> PlatformSyncs { get; set; }
    public virtual DbSet<SyncType> SyncTypes { get; set; }
    public virtual DbSet<Thumbnail> Thumbnails { get; set; }
    public virtual DbSet<ContentError> ContentErrors { get; set; }
    public virtual DbSet<ContentOtherSource> ContentOtherSources { get; set; }
}

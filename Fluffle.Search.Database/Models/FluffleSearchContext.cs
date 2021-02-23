using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Database;
using System;

namespace Noppes.Fluffle.Search.Database.Models
{
    public class DesignTimeContext : DesignTimeContext<FluffleSearchContext>
    {
    }

    public partial class FluffleSearchContext : BaseContext
    {
        public override Type ConfigurationType => typeof(SearchDatabaseConfiguration);

        public FluffleSearchContext()
        {
        }

        public FluffleSearchContext(DbContextOptions<FluffleSearchContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Content> Content { get; set; }
        public virtual DbSet<ContentFile> ContentFiles { get; set; }
        public virtual DbSet<ImageHash> ImageHashes { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<Platform> Platform { get; set; }
        public virtual DbSet<Thumbnail> Thumbnails { get; set; }
        public virtual DbSet<ContentCreditableEntity> ContentCreditableEntities { get; set; }
        public virtual DbSet<CreditableEntity> CreditableEntities { get; set; }
    }
}

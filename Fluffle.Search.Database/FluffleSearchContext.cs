using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Search.Database.Models;

namespace Noppes.Fluffle.Search.Database;

public class FluffleSearchContext : DbContext
{
    public FluffleSearchContext()
    {
    }

    public FluffleSearchContext(DbContextOptions<FluffleSearchContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("Relational:Collation", "en_US.utf8");

        base.OnModelCreating(modelBuilder);

        Platform.Configure(modelBuilder.Entity<Platform>());
        Image.Configure(modelBuilder.Entity<Image>());
        CreditableEntity.Configure(modelBuilder.Entity<CreditableEntity>());
        SearchRequest.Configure(modelBuilder.Entity<SearchRequest>());
    }

    public virtual DbSet<Image> Images { get; set; }
    public virtual DbSet<Platform> Platforms { get; set; }
    public virtual DbSet<CreditableEntity> CreditableEntities { get; set; }
    public virtual DbSet<SearchRequest> SearchRequests { get; set; }
}

using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Database;
using System;

namespace Noppes.Fluffle.Search.Database.Models;

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

    public virtual DbSet<ContentFile> ContentFiles { get; set; }
    public virtual DbSet<DenormalizedImage> DenormalizedImages { get; set; }
    public virtual DbSet<Platform> Platform { get; set; }
    public virtual DbSet<CreditableEntity> CreditableEntities { get; set; }
    public virtual DbSet<SearchRequestV2> SearchRequestsV2 { get; set; }
}

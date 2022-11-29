using Microsoft.EntityFrameworkCore;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.DeviantArt.Database.Entities;
using System;

namespace Noppes.Fluffle.DeviantArt.Database;

public class DeviantArtDesignTimeContext : DesignTimeContext<DeviantArtContext>
{
}

public class DeviantArtContext : BaseContext
{
    public override Type ConfigurationType => typeof(DeviantArtDatabaseConfiguration);

    public DeviantArtContext()
    {
    }

    public DeviantArtContext(DbContextOptions options) : base(options)
    {
    }

    public virtual DbSet<Deviant> Deviants { get; set; }
    public virtual DbSet<Deviation> Deviations { get; set; }
}
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Noppes.Fluffle.Utils;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Database
{
    /// <summary>
    /// The base class for all database context classes. Does some fancy reflecting magic on the
    /// <see cref="OnModelCreating"/> call to configure entities associated with the database
    /// automagically using the <see cref="IConfigurable{TEntity}"/> interface.
    /// </summary>
    public abstract class BaseContext : DbContext
    {
        /// <summary>
        /// This is the <see cref="Type"/> of the <b>configuration</b> associated with <b>this
        /// concrete implementation</b>.
        /// </summary>
        public abstract Type ConfigurationType { get; }

        protected BaseContext()
        {
        }

        protected BaseContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "en_US.utf8");

            base.OnModelCreating(modelBuilder);

            var entities = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(t => new
                {
                    Type = t,
                    Interface = t.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConfigurable<>))
                        .FirstOrDefault(i => i.GetGenericArguments()[0] == t)
                })
                .Where(x => x.Interface != null);

            var builderMethodInfo = typeof(ModelBuilder)
                .GetMethod(nameof(ModelBuilder.Entity), 1, Type.EmptyTypes);

            foreach (var entity in entities)
            {
                var instance = Activator.CreateInstance(entity.Type);

                var interfaceType = entity.Interface.GetGenericArguments()[0];

                var configureMethod = typeof(IConfigurable<>)
                    .MakeGenericType(interfaceType)
                    .GetMethod(nameof(IConfigurable<object>.Configure));

                var builderMethod = builderMethodInfo.MakeGenericMethod(interfaceType);

                var entityBuilder = builderMethod.Invoke(modelBuilder, Array.Empty<object>());
                configureMethod.Invoke(instance, new[] { entityBuilder });
            }

            // Get all of the entities which have been configured. We order these by their hierarchy
            // depth so that the base class always configured last.
            var entityTypes = modelBuilder.Model.GetEntityTypes()
                .Select(et => new
                {
                    EntityType = et,
                    Depth = et.ClrType.Depth()
                })
                .Where(et => !et.EntityType.IsKeyless)
                .OrderByDescending(et => et.Depth)
                .Select(et => et.EntityType);

            foreach (var entityType in entityTypes)
            {
                var tableName = entityType.ShortName().Underscore();

                // We use Table-per-Hierarchy (TPH) to deal with inheritance, so if the entity has
                // an entity base type, that means it's not the class at the top of the hierarchy.
                if (entityType.BaseType == null)
                    entityType.SetTableName(tableName);

                foreach (var property in entityType.GetProperties())
                {
                    var name = property.GetDefaultColumnBaseName().Underscore();
                    property.SetColumnName(name);
                }

                foreach (var key in entityType.GetKeys())
                {
                    var name = key.GetDefaultName().Underscore();
                    key.SetName(name);
                }

                foreach (var foreignKey in entityType.GetForeignKeys())
                {
                    // There are cases where we reference the same same table multiple times using
                    // multiple foreign keys. It's impossible to automatically generate these sames
                    // as that would cause them to end up having the same name, which is not
                    // allowed. So, if they foreign key has been given an explicit name (not the
                    // default name), then this name is used.
                    if (foreignKey.GetDefaultName() != foreignKey.GetConstraintName())
                        continue;

                    var name = "fk_" + foreignKey.PrincipalEntityType.GetTableName().Underscore();
                    foreignKey.SetConstraintName(name);
                }

                foreach (var index in entityType.GetIndexes())
                {
                    // Get the names properties making up the index
                    var properties = index.Properties
                        .Select(p => p.GetColumnName(StoreObjectIdentifier.Table(entityType.GetTableName(), entityType.GetSchema())));

                    // A unique constraint is actually handled using indexes by DBMSes, so instead
                    // prefixing a unique constraint with 'idx', we use 'uq'.
                    var namePrefix = index.IsUnique ? "uq" : "idx";
                    var name = $"{namePrefix}_{tableName}_{string.Join("_and_", properties)}";
                    index.SetDatabaseName(name);
                }
            }
        }

        // Down below we override all of the methods used to save changes and call a method to
        // update the dates at which entities are created and updated

        /// <inheritdoc />
        public override int SaveChanges()
        {
            ProcessTrackedEntities();

            return base.SaveChanges();
        }

        /// <inheritdoc />
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ProcessTrackedEntities();

            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        /// <inheritdoc />
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            ProcessTrackedEntities();

            return base.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            ProcessTrackedEntities();

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public void ProcessTrackedEntities()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (!(entry.Entity is TrackedBaseEntity entity))
                    continue;

                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = DateTime.Now;
                    continue;
                }

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.Now;
                    entity.UpdatedAt = entity.CreatedAt;
                }
            }
        }
    }
}

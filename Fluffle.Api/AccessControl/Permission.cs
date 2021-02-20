using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;
using System.Collections.Generic;

namespace Noppes.Fluffle.Api.AccessControl
{
    /// <summary>
    /// Generic database entity representing the most basic permission possible.
    /// </summary>
    public abstract class Permission<TApiKey, TPermission, TApiKeyPermission> : TrackedBaseEntity, IConfigurable<TPermission>
        where TApiKey : ApiKey<TApiKey, TPermission, TApiKeyPermission>
        where TPermission : Permission<TApiKey, TPermission, TApiKeyPermission>
        where TApiKeyPermission : ApiKeyPermission<TApiKey, TPermission, TApiKeyPermission>
    {
        protected Permission()
        {
            ApiKeyPermissions = new HashSet<TApiKeyPermission>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<TApiKeyPermission> ApiKeyPermissions { get; set; }

        public void Configure(EntityTypeBuilder<TPermission> entity)
        {
            entity.Property(e => e.Id);
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(64);

            entity.HasIndex(e => e.Name).IsUnique();
        }
    }
}

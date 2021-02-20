using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;
using System.Collections.Generic;

namespace Noppes.Fluffle.Api.AccessControl
{
    /// <summary>
    /// Generic database entity representing the most basic API key possible.
    /// </summary>
    public abstract class ApiKey<TApiKey, TPermission, TApiKeyPermission> : TrackedBaseEntity, IConfigurable<TApiKey>
        where TApiKey : ApiKey<TApiKey, TPermission, TApiKeyPermission>
        where TPermission : Permission<TApiKey, TPermission, TApiKeyPermission>
        where TApiKeyPermission : ApiKeyPermission<TApiKey, TPermission, TApiKeyPermission>
    {
        protected ApiKey()
        {
            ApiKeyPermissions = new HashSet<TApiKeyPermission>();
        }

        public int Id { get; set; }

        public string Key { get; set; }

        public string Description { get; set; }

        public virtual ICollection<TApiKeyPermission> ApiKeyPermissions { get; set; }

        public void Configure(EntityTypeBuilder<TApiKey> entity)
        {
            entity.Property(e => e.Id);
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Description)
                .IsRequired()
                .HasColumnType("character varying");

            entity.Property(e => e.Key)
                .IsRequired()
                .HasMaxLength(32)
                .IsFixedLength();

            entity.HasIndex(e => e.Key)
                .IsUnique();
        }
    }
}

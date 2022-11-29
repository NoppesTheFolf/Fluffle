using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Noppes.Fluffle.Database.KeyValue
{
    public abstract class KeyValuePair : BaseEntity, IConfigurable<KeyValuePair>
    {
        public string Key { get; set; }

        public byte[] Value { get; set; }

        public void Configure(EntityTypeBuilder<KeyValuePair> entity)
        {
            entity.Property(x => x.Key).IsRequired();
            entity.HasKey(x => x.Key);

            entity.Property(x => x.Value);
        }
    }
}

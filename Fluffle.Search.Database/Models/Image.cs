using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noppes.Fluffle.Database;

namespace Noppes.Fluffle.Search.Database.Models
{
    public partial class Image : Content, IConfigurable<Image>
    {
        public virtual ImageHash ImageHash { get; set; }

        public void Configure(EntityTypeBuilder<Image> entity)
        {
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Noppes.Fluffle.Database
{
    /// <summary>
    /// Entities can inherit from this class with their own type, meaning they can configure
    /// themselves. This keeps the associated <see cref="DbContext"/> from getting an awfully clumsy
    /// to work with.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IConfigurable<TEntity> where TEntity : class
    {
        public void Configure(EntityTypeBuilder<TEntity> entity);
    }
}

using System;

namespace Noppes.Fluffle.Main.Api.Helpers
{
    public interface IDataSource<in TEnum, out TEntity> where TEnum : Enum where TEntity : class, new()
    {
        public TEntity From(TEnum value);
    }
}

using System;

namespace Noppes.Fluffle.Api.Mapping
{
    public class MapperNotFoundException : Exception
    {
        public MapperNotFoundException(Type srcType, Type destType) : base(
            $"There exists no mapper which is able to map from {srcType.Name} to {destType.Name}.")
        {
        }
    }
}

using System;

namespace Noppes.Fluffle.Api.Mapping
{
    public class MapperMissingPublicConstructorException : Exception
    {
        public MapperMissingPublicConstructorException(Type type)
            : base($"Mapper of type {type.Name} must have a public parameterless constructor.")
        {
        }
    }
}

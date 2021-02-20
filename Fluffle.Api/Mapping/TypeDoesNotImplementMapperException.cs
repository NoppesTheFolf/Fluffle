using System;

namespace Noppes.Fluffle.Api.Mapping
{
    public class TypeDoesNotImplementMapperException : Exception
    {
        public TypeDoesNotImplementMapperException(Type type)
            : base($"Type {type.Name} does not implement {typeof(IMapper<,>).Name}")
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Noppes.Fluffle.Api.Mapping
{
    /// <summary>
    /// Provides extension methods for <see cref="IMapperer{TDest,TSrc}"/> types to make mapping
    /// just a bit more convenient.
    /// </summary>
    public static class Mappers
    {
        /// <summary>
        /// A cache used by the <see cref="GetMapper{TDest}"/> methods. It uses reflection to create
        /// instances of classes that implement <see cref="IMapper{TSrc,TDest}"/> dynamically.
        /// Reflection is a relatively expensive operation and caching should therefore be used
        /// whenever possible.
        /// </summary>
        private static readonly Dictionary<(Type, Type), dynamic> MapperCache = new Dictionary<(Type, Type), dynamic>();

        /// <summary>
        /// Finds and initializes all the classes which implement <see cref="IMapper{TSrc,TDest}"/>.
        /// </summary>
        public static void Initialize()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes());

            Initialize(types);
        }

        /// <summary>
        /// Initialize the given class (type) which implements <see cref="IMapper{TSrc,TDest}"/>. An
        /// <see cref="TypeDoesNotImplementMapperException"/> will be thrown if the given class
        /// doesn't <see cref="IMapper{TSrc,TDest}"/>.
        /// </summary>
        public static void Initialize(Type type)
        {
            var initializedCount = Initialize(new[] { type });

            if (initializedCount == 0)
                throw new TypeDoesNotImplementMapperException(type);
        }

        /// <summary>
        /// Initialize the given classes (types) which implements <see cref="IMapper{TSrc,TDest}"/>.
        /// Types which don't implement <see cref="IMapper{TSrc,TDest}"/> are ignored.
        /// </summary>
        public static int Initialize(IEnumerable<Type> types)
        {
            var mappableTypes = types
                .Where(t => !t.IsAbstract)
                .Select(t => new
                {
                    MapperType = t,
                    Mappable = t.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapper<,>))
                        .ToList()
                })
                .Where(t => t.Mappable.Any())
                .ToList();

            foreach (var mappableType in mappableTypes)
            {
                foreach (var mapperInterface in mappableType.Mappable)
                {
                    var interfaceArguments = mapperInterface.GetGenericArguments();
                    var srcType = interfaceArguments[0];
                    var destType = interfaceArguments[1];

                    Initialize(mappableType.MapperType, srcType, destType);
                }
            }

            return mappableTypes.Sum(mt => mt.Mappable.Count);
        }

        /// <summary>
        /// Maps the given object to provided type. Make sure the <see cref="IMapper{TDest,TSrc}"/>
        /// interface is implemented correctly. Bad things, also knows as runtime error, will occur
        /// if you call this method without having implemented to proper methods.
        /// </summary>
        /// <typeparam name="TDest">The type which to map to</typeparam>
        public static TDest MapTo<TDest>(this object src)
        {
            var dest = Activator.CreateInstance<TDest>();

            src.MapTo(dest);

            return dest;
        }

        public static void MapTo<TDest>(this object src, TDest dest)
        {
            var mapDelegate = GetMapper<TDest>(src);

            mapDelegate((dynamic)src, dest);
        }

        public static void MapTo<TSrc, TDest>(this TSrc src, TDest dest)
        {
            var mapDelegate = GetMapper(typeof(TSrc), typeof(TDest));

            mapDelegate((dynamic)src, dest);
        }

        /// <summary>
        /// Maps the given objects to provided type. Make sure the <see cref="IMapper{TSrc,TDest}"/>
        /// interface is implemented correctly. Bad things, also knows as runtime error, will occur
        /// if you call this method without having implemented to proper interface.
        /// </summary>
        /// <typeparam name="TDest">The type which to map to</typeparam>
        public static IEnumerable<TDest> MapEnumerableTo<TDest>(this IEnumerable<object> objs)
        {
            if (objs == null)
                throw new ArgumentNullException(nameof(objs));

            var enumerableType = objs.GetType().GetGenericArguments()[0];
            var mapMethod = GetMapper<TDest>(enumerableType);
            foreach (var obj in objs)
            {
                var dest = Activator.CreateInstance<TDest>();
                mapMethod((dynamic)obj, dest);
                yield return dest;
            }
        }

        /// <summary>
        /// Maps the given instances of type <typeparamref name="TSrc"/> to <typeparamref
        /// name="TDest"/>. This method should only be used when you need to map a <see
        /// cref="IEnumerable{T}"/> which contains structs (for example <see cref="int"/> and <see
        /// cref="string"/>), as those aren't objects and therefore can't be mapped using <see
        /// cref="MapEnumerableTo{TDest}"/>. Make sure the <see cref="IMapper{TSrc,TDest}"/>
        /// interface is implemented correctly. Bad things, also knows as runtime error, will occur
        /// if you call this method without having implemented to proper interface.
        /// </summary>
        /// <typeparam name="TSrc">The source enumerable its type</typeparam>
        /// <typeparam name="TDest">The type which to map to</typeparam>
        public static IEnumerable<TDest> MapEnumerableTo<TSrc, TDest>(this IEnumerable<TSrc> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var mapMethod = GetMapper<TDest>(typeof(TSrc));
            foreach (var value in values)
                yield return mapMethod((dynamic)value);
        }

        private static void Initialize(Type mapperInterfaceType, Type srcType, Type destType)
        {
            var expectedParameters = new[] { srcType, destType };

            var mapMethod = mapperInterfaceType
                .GetMethods()
                .Where(m => m.Name == nameof(IMapper<object, object>.MapFrom))
                .Where(m => m.ReturnType == typeof(void))
                .Where(m => m.GetParameters().Length == 2)
                .Where(m => m.GetParameters().First().ParameterType == srcType)
                .First(m => m.GetParameters().Skip(1).First().ParameterType == destType);

            AddToCache(mapperInterfaceType, srcType, destType, mapMethod);
        }

        private static void AddToCache(Type mapperType, Type srcType, Type destType, MethodInfo mapMethod)
        {
            // Skip mappers being added to the cache twice
            if (MapperCache.ContainsKey((srcType, destType)))
                return;

            // The given type needs a parameterless constructor for us to be able to create an
            // instance of it
            var hasParameterlessConstructor = mapperType.GetConstructors()
                .Any(c => c.GetParameters().Length == 0 && !c.ContainsGenericParameters);

            if (!hasParameterlessConstructor)
                throw new MapperMissingPublicConstructorException(mapperType);

            // We create a delegate which can be invoked later. This is more efficient than calling
            // Invoke on an instance of MethodInfo
            var mapperInstance = Activator.CreateInstance(mapperType);
            var delegateType = typeof(Action<,>).MakeGenericType(srcType, destType);
            var mapDelegate = Delegate.CreateDelegate(delegateType, mapperInstance, mapMethod);

            MapperCache.Add((srcType, destType), mapDelegate);
        }

        /// <summary>
        /// Gets the mapper which implements <see cref="IMapper{TMappingSrc,TMappingDest}"/> where <typeparamref
        /// name="TMappingSrc"/> is the type of the given object <paramref name="obj"/> and <typeparamref name="TMappingDest"/> is
        /// <typeparamref name="TDest"/>.
        /// </summary>
        private static dynamic GetMapper<TDest>(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return GetMapper(obj.GetType(), typeof(TDest));
        }

        /// <summary>
        /// Gets the mapper which implements <see cref="IMapper{TMappingSrc,TMappingDest}"/> where <typeparamref
        /// name="TMappingSrc"/> is <paramref name="srcType"/> and <typeparamref name="TMappingDest"/> is
        /// <typeparamref name="TDest"/>.
        /// </summary>
        private static dynamic GetMapper<TDest>(Type srcType)
        {
            return GetMapper(srcType, typeof(TDest));
        }

        /// <summary>
        /// Gets the mapper which implements <see cref="IMapper{TSrc,TDest}"/> where <typeparamref
        /// name="TSrc"/> is <paramref name="srcType"/> and <typeparamref name="TDest"/> is
        /// <paramref name="destType"/>.
        /// </summary>
        private static dynamic GetMapper(Type srcType, Type destType)
        {
            // Check if the cache contains an entry
            if (MapperCache.TryGetValue((srcType, destType), out var mapper))
                return mapper;

            throw new MapperNotFoundException(srcType, destType);
        }
    }
}

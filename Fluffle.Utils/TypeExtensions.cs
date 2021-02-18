using System;

namespace Noppes.Fluffle.Utils
{
    /// <summary>
    /// Extension methods for objects of type <see cref="Type"/>.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets the depth of the given type when looking its hierarchy (base classes). Due to all
        /// classes (other than <see cref="object"/> of course) inheriting from <see
        /// cref="object"/>, this method will generally return at least 1.
        /// </summary>
        public static int Depth(this Type type) => Depth(type, 0);

        private static int Depth(this Type type, int depth)
        {
            if (type.BaseType == null)
                return depth;

            return Depth(type.BaseType, depth + 1);
        }
    }
}

using System.Text;

namespace Noppes.Fluffle.Database
{
    public static class Extensions
    {
        /// <summary>
        /// String containing the unicode NULL character.
        /// </summary>
        public static readonly string NullChar = Encoding.UTF8.GetString(new byte[] { 0x00 });

        /// <summary>
        /// Removes all unicode NULL characters from the given string.
        /// </summary>
        public static string RemoveNullChar(this string value) => value.Replace(NullChar, string.Empty);
    }
}

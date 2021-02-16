using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Noppes.Fluffle.Utils
{
    /// <summary>
    /// Provides some convenient ways to generate MD5 and SHA1 hashes.
    /// </summary>
    public static class Hashing
    {
        /// <summary>
        /// Computes the MD5 hash of the provided ASCII encoded string.
        /// </summary>
        public static string Md5(string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);

            return Md5(bytes);
        }

        /// <summary>
        /// Computes the MD5 hash for the given array of bytes.
        /// </summary>
        public static string Md5(byte[] bytes)
        {
            using var md5 = MD5.Create();

            return BytesToString(md5.ComputeHash(bytes));
        }

        /// <summary>
        /// Computes the SHA1 hash for the given stream.
        /// </summary>
        public static string Sha1(Stream stream)
        {
            using var sha1 = SHA1.Create();

            return BytesToString(sha1.ComputeHash(stream));
        }

        private static string BytesToString(ReadOnlySpan<byte> bytes)
        {
            var hashBuilder = new StringBuilder();

            foreach (var part in bytes)
                hashBuilder.Append(part.ToString("x2"));

            return hashBuilder.ToString();
        }
    }
}

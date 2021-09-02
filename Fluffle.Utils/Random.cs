using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Noppes.Fluffle.Utils
{
    public static class Random
    {
        private static readonly Regex AllowedCharacter = new("[a-zA-Z0-9]", RegexOptions.Compiled);

        /// <summary>
        /// Generates a random string of the specified length in a secure manner.
        /// </summary>
        public static string GenerateString(int length)
        {
            if (length < 1)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be equal to or greater than 1.");

            var buffer = new byte[1];
            var rngStr = new char[length];
            using var rng = new RNGCryptoServiceProvider();
            for (var i = 0; i < length; i++)
            {
                while (true)
                {
                    rng.GetBytes(buffer);
                    var character = (char)buffer[0];

                    if (!AllowedCharacter.IsMatch(char.ToString(character)))
                        continue;

                    rngStr[i] = character;
                    break;
                }
            }

            return new string(rngStr);
        }
    }
}

﻿using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Noppes.Fluffle.Utils;

public static class RandomString
{
    private static readonly Regex AllowedCharacter = new("[a-zA-Z0-9]", RegexOptions.Compiled);

    /// <summary>
    /// Generates a random string of the specified length in a secure manner.
    /// </summary>
    public static string Generate(int length)
    {
        if (length < 1)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be equal to or greater than 1.");

        var rngStr = new char[length];
        for (var i = 0; i < length; i++)
        {
            while (true)
            {
                var buffer = RandomNumberGenerator.GetBytes(1);
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

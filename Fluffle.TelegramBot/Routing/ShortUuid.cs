using System.Security.Cryptography;

namespace Fluffle.TelegramBot.Routing;

public static class ShortUuid
{
    private const string Alphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

    public static string Random(int length)
    {
        var chars = new char[length];

        for (var i = 0; i < length; i++)
        {
            var alphabetIndex = RandomNumberGenerator.GetInt32(0, Alphabet.Length);
            chars[i] = Alphabet[alphabetIndex];
        }

        return new string(chars);
    }
}

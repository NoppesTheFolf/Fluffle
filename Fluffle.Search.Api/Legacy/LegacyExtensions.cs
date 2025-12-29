using System.Text.RegularExpressions;

namespace Fluffle.Search.Api.Legacy;

public static class LegacyExtensions
{
    public static string Pascalize(this string input)
    {
        return Regex.Replace(input, "(?:^|_| +)(.)", match => match.Groups[1].Value.ToUpper());
    }

    public static string Camelize(this string input)
    {
        var word = input.Pascalize();
        return word.Length > 0 ? word[..1].ToLower() + word[1..] : word;
    }
}

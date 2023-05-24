using System.Collections.Generic;
using System.Globalization;

namespace Noppes.Fluffle.Utils;

public static class StringExtensions
{
    public static IEnumerable<string> EnumerateGraphemes(this string value)
    {
        var enumerator = StringInfo.GetTextElementEnumerator(value);
        while (enumerator.MoveNext())
        {
            yield return enumerator.GetTextElement();
        }
    }
}

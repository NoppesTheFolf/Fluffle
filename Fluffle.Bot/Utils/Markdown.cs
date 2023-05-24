using System;
using System.ComponentModel;
using Telegram.Bot.Types.Enums;

namespace Noppes.Fluffle.Bot.Utils;

public static class Markdown
{
    public static string Escape(string value, ParseMode parseMode, MessageEntityType? entityType = null)
    {
        if (parseMode is not ParseMode.Markdown and not ParseMode.MarkdownV2)
            throw new InvalidEnumArgumentException(nameof(parseMode));

        var escapeChars = parseMode switch
        {
            ParseMode.Markdown => "\\`",
            ParseMode.MarkdownV2 => entityType switch
            {
                MessageEntityType.Pre or MessageEntityType.Code => "\\`",
                MessageEntityType.TextLink => "\\)",
                _ => "_*[]()~`>#+-=|{}.!"
            },
            _ => throw new ArgumentOutOfRangeException(nameof(parseMode), parseMode, null)
        };

        foreach (var escapeChar in escapeChars)
            value = value.Replace(escapeChar.ToString(), $"\\{escapeChar}");

        return value;
    }
}

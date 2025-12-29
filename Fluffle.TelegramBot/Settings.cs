using Fluffle.TelegramBot.Routing.InlineKeyboard;

namespace Fluffle.TelegramBot;

public enum ReverseSearchFormat
{
    [InlineKeyboardText("Inline keyboard")]
    InlineKeyboard = 1,
    [InlineKeyboardText("Text")]
    Text = 2
}

public enum TextFormat
{
    [InlineKeyboardText("Platform names")]
    PlatformNames = 1,
    [InlineKeyboardText("Compact links")]
    Compact = 2,
    [InlineKeyboardText("Expanded links")]
    Expanded = 3
}

public enum InlineKeyboardFormat
{
    [InlineKeyboardText("Single source")]
    Single = 1,
    [InlineKeyboardText("Multiple sources")]
    Multiple = 2
}

public enum TextSeparator
{
    [InlineKeyboardText("Vertical bar |")]
    VerticalBar = 1,
    [InlineKeyboardText("Forward slash /")]
    ForwardSlash = 2,
    [InlineKeyboardText("Bullet •")]
    Bullet = 3,
    [InlineKeyboardText("A custom one")]
    Custom = 4
}

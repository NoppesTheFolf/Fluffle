using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Fluffle.TelegramBot.Database;
using Fluffle.TelegramBot.Database.Entities;
using Fluffle.TelegramBot.Routing;
using Fluffle.TelegramBot.Routing.Extensions;
using Fluffle.TelegramBot.Routing.InlineKeyboard;
using Fluffle.TelegramBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Fluffle.TelegramBot.Controllers;

public interface IChatCallbackQueryData
{
    public long ChatId { get; init; }
}

public record SetFormatCallbackQueryData : IChatCallbackQueryData
{
    public long ChatId { get; init; }

    public ReverseSearchFormat ReverseSearchFormat { get; set; }
}

public record SetTextFormatCallbackQueryData : IChatCallbackQueryData
{
    public long ChatId { get; init; }

    public TextFormat TextFormat { get; set; }
}

public record SetSetTextSeparatorData : IChatCallbackQueryData
{
    public long ChatId { get; init; }

    public TextSeparator TextSeparator { get; init; }
}

public record SetInlineKeyboardFormatCallbackQueryData : IChatCallbackQueryData
{
    public long ChatId { get; init; }

    public InlineKeyboardFormat InlineKeyboardFormat { get; init; }
}

public class SettingsMenuController
{
    private readonly ITelegramBotClient _botClient;
    private readonly CallbackManager _callbackManager;
    private readonly InputManager _inputManager;
    private readonly BotContext _context;

    public SettingsMenuController(ITelegramBotClient botClient, CallbackManager callbackManager, InputManager inputManager, BotContext context)
    {
        _botClient = botClient;
        _callbackManager = callbackManager;
        _inputManager = inputManager;
        _context = context;
    }

    #region SetFormat

    [Command("setformat")]
    public async Task RequestSetFormat(Message message)
    {
        await SelectChat(message.Chat, message.From, x => new SetFormatCallbackQueryData { ChatId = x }, ChooseFormat, "For which chat would you like to set the reverse search format?", nameof(SettingsMenuController), nameof(ChooseFormat));
    }

    [CallbackQuery]
    public async Task ChooseFormat(CallbackQuery callbackQuery, SetFormatCallbackQueryData data) => await ChooseFormat(callbackQuery.Message!.Chat, data);

    private async Task ChooseFormat(Chat chat, SetFormatCallbackQueryData data)
    {
        await PresentEnumOptions<ReverseSearchFormat, SetFormatCallbackQueryData>(chat, x => data with { ReverseSearchFormat = x }, "Which reverse search format would you like to use?", nameof(SettingsMenuController), nameof(SetFormat));
    }

    [CallbackQuery]
    public async Task SetFormat(CallbackQuery callbackQuery, SetFormatCallbackQueryData data)
    {
        await SetChatSetting(callbackQuery, data, chat => chat.ReverseSearchFormat = data.ReverseSearchFormat, chat => $"Reverse search format of {chat} set to _{Markdown.Escape(data.ReverseSearchFormat.InlineKeyboardText(), ParseMode.MarkdownV2)}_\\.");
    }

    #endregion

    #region SetInlineKeyboardFormat

    [Command("setinlinekeyboardformat")]
    public async Task RequestSetInlineKeyboardFormat(Message message)
    {
        await SelectChat(message.Chat, message.From, x => new SetInlineKeyboardFormatCallbackQueryData { ChatId = x }, ChooseInlineKeyboardFormat, "For which chat would you like to set the inline keyboard format?", nameof(SettingsMenuController), nameof(ChooseInlineKeyboardFormat));
    }

    [CallbackQuery]
    public async Task ChooseInlineKeyboardFormat(CallbackQuery callbackQuery, SetInlineKeyboardFormatCallbackQueryData data) => await ChooseInlineKeyboardFormat(callbackQuery.Message!.Chat, data);

    public async Task ChooseInlineKeyboardFormat(Chat chat, SetInlineKeyboardFormatCallbackQueryData data)
    {
        await PresentEnumOptions<InlineKeyboardFormat, SetInlineKeyboardFormatCallbackQueryData>(chat, x => data with { InlineKeyboardFormat = x }, "Which inline keyboard format would you like to use?", nameof(SettingsMenuController), nameof(SetInlineKeyboardFormat));
    }

    [CallbackQuery]
    public async Task SetInlineKeyboardFormat(CallbackQuery callbackQuery, SetInlineKeyboardFormatCallbackQueryData data)
    {
        await SetChatSetting(callbackQuery, data, chat => chat.InlineKeyboardFormat = data.InlineKeyboardFormat, chat => $"Inline keyboard format of {chat} set to _{Markdown.Escape(data.InlineKeyboardFormat.InlineKeyboardText(), ParseMode.MarkdownV2)}_\\.");
    }

    #endregion

    #region SetTextFormat

    [Command("settextformat")]
    public async Task RequestSetTextFormat(Message message)
    {
        await SelectChat(message.Chat, message.From, x => new SetTextFormatCallbackQueryData { ChatId = x }, ChooseTextFormat, "For which chat would you like to set the text format?", nameof(SettingsMenuController), nameof(ChooseTextFormat));
    }

    [CallbackQuery]
    public async Task ChooseTextFormat(CallbackQuery callbackQuery, SetTextFormatCallbackQueryData data) => await ChooseTextFormat(callbackQuery.Message!.Chat, data);

    public async Task ChooseTextFormat(Chat chat, SetTextFormatCallbackQueryData data)
    {
        await PresentEnumOptions<TextFormat, SetTextFormatCallbackQueryData>(chat, x => data with { TextFormat = x }, "Which text format would you like to use?", nameof(SettingsMenuController), nameof(SetTextFormat));
    }

    [CallbackQuery]
    public async Task SetTextFormat(CallbackQuery callbackQuery, SetTextFormatCallbackQueryData data)
    {
        await SetChatSetting(callbackQuery, data, chat => chat.TextFormat = data.TextFormat, chat => $"Text format of {chat} set to _{Markdown.Escape(data.TextFormat.InlineKeyboardText(), ParseMode.MarkdownV2)}_\\.");
    }

    #endregion

    #region SetTextSeparator

    [Command("settextseparator")]
    public async Task RequestSetTextSeparator(Message message)
    {
        await SelectChat(message.Chat, message.From, x => new SetSetTextSeparatorData { ChatId = x }, ChooseTextSeparator, "For which chat would you like to set the text separator?", nameof(SettingsMenuController), nameof(ChooseTextSeparator));
    }

    [CallbackQuery]
    public async Task ChooseTextSeparator(CallbackQuery callbackQuery, SetSetTextSeparatorData data) => await ChooseTextSeparator(callbackQuery.Message!.Chat, data);

    public async Task ChooseTextSeparator(Chat chat, SetSetTextSeparatorData data)
    {
        await PresentEnumOptions<TextSeparator, SetSetTextSeparatorData>(chat, x => data with { TextSeparator = x }, "Which separator would you like to use?", nameof(SettingsMenuController), nameof(SetTextSeparator));
    }

    [CallbackQuery]
    public async Task SetTextSeparator(CallbackQuery callbackQuery, SetSetTextSeparatorData data)
    {
        if (data.TextSeparator == TextSeparator.Custom)
        {
            await _inputManager.CreateAsync(callbackQuery.Message!.Chat.Id, data, nameof(SettingsMenuController), nameof(SetTextSeparator));
            await RateLimiter.RunAsync(callbackQuery.Message.Chat, () => _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Which character would you like to use a custom separator? Please type it in the chat. This may be anything including emojis and other unicode symbols."));
            return;
        }

        await SetSetTextSeparator(callbackQuery.Message, data, data.TextSeparator switch
        {
            TextSeparator.VerticalBar => "|",
            TextSeparator.ForwardSlash => "/",
            TextSeparator.Bullet => "•",
            _ => throw new ArgumentOutOfRangeException()
        });
    }

    [Input]
    public async Task SetTextSeparator(Message message, SetSetTextSeparatorData data)
    {
        if (EnumerateGraphemes(message.Text).Count() != 1)
        {
            await _inputManager.CreateAsync(message.Chat.Id, data, nameof(SettingsMenuController), nameof(SetTextSeparator));
            await RateLimiter.RunAsync(message.Chat, () => _botClient.SendTextMessageAsync(message.Chat.Id, "A separator may only consist out of one character. Please try again."));
            return;
        }

        await SetSetTextSeparator(message, data, message.Text);
    }

    private static IEnumerable<string> EnumerateGraphemes(string value)
    {
        var enumerator = StringInfo.GetTextElementEnumerator(value);
        while (enumerator.MoveNext())
        {
            yield return enumerator.GetTextElement();
        }
    }

    private async Task SetSetTextSeparator(Message message, SetSetTextSeparatorData data, string separator)
    {
        await SetChatSetting(message, data, chat => chat.TextSeparator = separator, chat => $"Text separator of {chat} set to _{Markdown.Escape(separator, ParseMode.MarkdownV2)}_\\.");
    }

    #endregion

    public async Task SetChatSetting<T>(CallbackQuery callbackQuery, T data, Action<MongoChat> updateChat, Func<string, string> getText) where T : IChatCallbackQueryData
    {
        await SetChatSetting(callbackQuery.Message, data, updateChat, getText);
    }

    public async Task SetChatSetting<T>(Message message, T data, Action<MongoChat> updateChat, Func<string, string> getText) where T : IChatCallbackQueryData
    {
        await SetChatSetting(message.Chat, message.From, data, updateChat, getText);
    }

    public async Task SetChatSetting<T>(Chat tgChat, User user, T data, Action<MongoChat> updateChat, Func<string, string> getText) where T : IChatCallbackQueryData
    {
        var chat = await _context.Chats.FirstAsync(x => x.Id == data.ChatId);
        var chatOwner = tgChat.Type == ChatType.Private
            ? user
            : await _botClient.GetChatOwnerAsync(data.ChatId);

        var chatString = chat.Id == tgChat.Id
            ? "_this chat_" : $"chat _{Markdown.Escape(chat.Title, ParseMode.MarkdownV2)}_";

        if (chatOwner.Id != user.Id)
        {
            await RateLimiter.RunAsync(tgChat, () => _botClient.SendTextMessageAsync(tgChat.Id, $"You are not allowed to make any changes to {chatString} because you are not the owner anymore\\.", ParseMode.MarkdownV2));

            chat.OwnerId = chatOwner.Id;
            await _context.Chats.ReplaceAsync(x => x.Id == chat.Id, chat);

            return;
        }

        await RateLimiter.RunAsync(tgChat, () => _botClient.SendTextMessageAsync(tgChat.Id, getText(chatString), ParseMode.MarkdownV2));

        updateChat(chat);
        await _context.Chats.ReplaceAsync(x => x.Id == chat.Id, chat);
    }

    private async Task SelectChat<TData>(Chat chat, User owner, Func<long, TData> getData, Func<Chat, TData, Task> next, string text, string controller, string action) where TData : IChatCallbackQueryData
    {
        var chats = await _context.Chats.GetOwnedChatsAsync(owner.Id);

        if (chats.Count == 0)
            return;

        if (chats.All(x => x.id == owner.Id))
        {
            await next(chat, getData(chats[0].id));
            return;
        }

        var keyboard = await _callbackManager.CreateAsync(chats.Select(x => (x.title, getData(x.id))).ToList(), controller, action);
        await RateLimiter.RunAsync(chat, () => _botClient.SendTextMessageAsync(chat.Id, text, replyMarkup: keyboard));
    }

    private async Task PresentEnumOptions<TEnum, TData>(Chat chat, Func<TEnum, TData> getData, string text, string controller, string action) where TEnum : struct, Enum
    {
        var values = Enum.GetValues<TEnum>()
            .Select(x => (x.InlineKeyboardText(), getData(x)))
            .ToList();

        var keyboard = await _callbackManager.CreateAsync(values, controller, action);
        await RateLimiter.RunAsync(chat, () => _botClient.SendTextMessageAsync(chat.Id, text, ParseMode.MarkdownV2, replyMarkup: keyboard));
    }
}

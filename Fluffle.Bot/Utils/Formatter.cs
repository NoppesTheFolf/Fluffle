using Noppes.Fluffle.Bot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Noppes.Fluffle.Bot.Utils;

public static class Formatter
{
    public const string SourcesText = "Sources";

    public static void RouteMessage(MongoMessage message, ReverseSearchResponse response)
    {
        Action<MongoMessage, ReverseSearchResponse> routeAction = message.ReverseSearchFormat switch
        {
            ReverseSearchFormat.Text => TextFormatter.RouteMessage,
            ReverseSearchFormat.InlineKeyboard => InlineKeyboardFormatter.RouteMessage,
            _ => throw new ArgumentOutOfRangeException()
        };

        routeAction(message, response);
    }

    public static void RouteMediaGroup(string url, MongoMessage message, ReverseSearchResponse response)
    {
        Action<string, MongoMessage, ReverseSearchResponse> routeAction = message.ReverseSearchFormat switch
        {
            ReverseSearchFormat.Text => TextFormatter.RouteMediaGroup,
            ReverseSearchFormat.InlineKeyboard => InlineKeyboardFormatter.RouteMediaGroup,
            _ => throw new ArgumentOutOfRangeException()
        };

        routeAction(url, message, response);
    }
}

public static class TextFormatter
{
    public static void RouteMediaGroup(string url, MongoMessage message, ReverseSearchResponse response)
    {
        (string text, ICollection<MessageEntity> textEntities, bool shouldCaptionBeAfter) x = message.TextFormat switch
        {
            TextFormat.PlatformNames => (Formatter.SourcesText, new List<MessageEntity> { new() { Url = url, Offset = 0, Length = Formatter.SourcesText.Length, Type = MessageEntityType.TextLink } }, false),
            TextFormat.Compact or TextFormat.Expanded => (url, Array.Empty<MessageEntity>(), true),
            _ => throw new ArgumentOutOfRangeException()
        };

        SetText(response, x.text, x.textEntities, x.shouldCaptionBeAfter);
    }

    private const int LinksLimit = 3;

    public static void RouteMessage(MongoMessage message, ReverseSearchResponse response)
    {
        var (text, textEntities, shouldCaptionBeAfter) = message.TextFormat switch
        {
            TextFormat.PlatformNames => PlatformNames(message, message.FluffleResponse),
            TextFormat.Compact => Links(message.FluffleResponse, false),
            TextFormat.Expanded => Links(message.FluffleResponse, true),
            _ => throw new ArgumentOutOfRangeException()
        };

        SetText(response, text, textEntities, shouldCaptionBeAfter);
    }

    public static void SetText(ReverseSearchResponse response, string text, ICollection<MessageEntity> textEntities, bool shouldCaptionBeAfter)
    {
        if (response.ExistingText != null)
        {
            const string nextLine = "\n\n";
            var prefix = shouldCaptionBeAfter ? string.Empty : text + nextLine;
            var suffix = shouldCaptionBeAfter ? nextLine + text : string.Empty;

            void OffsetMessageEntities(IEnumerable<MessageEntity> messageEntities, int offset)
            {
                foreach (var textEntity in messageEntities)
                    textEntity.Offset += offset;
            }

            if (prefix != string.Empty && response.ExistingTextEntities != null)
                OffsetMessageEntities(response.ExistingTextEntities, prefix.Length);

            if (suffix != string.Empty && response.ExistingText != null)
                OffsetMessageEntities(textEntities, response.ExistingText.Length);

            text = prefix + response.ExistingText + suffix;
        }

        response.TextEntities = textEntities.Concat(response.ExistingTextEntities ?? Array.Empty<MessageEntity>()).ToArray();
        response.Text = text;
    }

    public static (string, ICollection<MessageEntity>, bool) PlatformNames(MongoMessage message, FluffleResponse response)
    {
        var text = string.Empty;
        var messageEntities = new List<MessageEntity>();

        var offset = 0;
        for (var i = 0; i < response.Results.Count; i++)
        {
            var result = response.Results[i];
            var platformName = result.Platform.Pretty();
            text += platformName;

            messageEntities.Add(new MessageEntity
            {
                Type = MessageEntityType.TextLink,
                Offset = offset,
                Length = platformName.Length,
                Url = result.Location
            });

            if (i < response.Results.Count - 1)
                text += $" {message.TextSeparator} ";

            offset = text.Length;
        }

        return (text, messageEntities, false);
    }

    public static (string, ICollection<MessageEntity>, bool) Links(FluffleResponse response, bool expanded)
    {
        var text = string.Empty;
        var messageEntities = new List<MessageEntity>();
        var results = response.Results.Take(LinksLimit).ToList();
        for (var i = 0; i < results.Count; i++)
        {
            var result = results[i];

            if (expanded)
            {
                var platformName = result.Platform.Pretty();
                messageEntities.Add(new MessageEntity
                {
                    Type = MessageEntityType.Bold,
                    Offset = text.Length,
                    Length = platformName.Length
                });

                text += platformName;
                text += '\n';
            }

            text += result.Location;
            text += '\n';

            if (expanded && i < results.Count - 1)
                text += '\n';
        }

        return (text, messageEntities, true);
    }
}

public static class InlineKeyboardFormatter
{
    private const string FallbackText = "🦊🔍...";
    private const int MaxRows = 2;

    private record BinOption(int BinSize, int CompartmentSize);

    public static void RouteMediaGroup(string url, MongoMessage message, ReverseSearchResponse response)
    {
        RouteSingle(url, Formatter.SourcesText, response);
        AppendTextIfNeeded(response);
    }

    public static void RouteMessage(MongoMessage message, ReverseSearchResponse response)
    {
        Action<MongoMessage, ReverseSearchResponse> routeAction = message.InlineKeyboardFormat switch
        {
            InlineKeyboardFormat.Single => (x, y) => RouteSingle(x.FluffleResponse.Results.First().Location, "Source", y),
            InlineKeyboardFormat.Multiple => RouteMultiple,
            _ => throw new ArgumentOutOfRangeException()
        };

        routeAction(message, response);
        AppendTextIfNeeded(response);
    }

    private static void AppendTextIfNeeded(ReverseSearchResponse response)
    {
        if (response.Chat.Type is ChatType.Group or ChatType.Supergroup)
            response.Text ??= FallbackText;
    }

    private static void RouteSingle(string url, string text, ReverseSearchResponse response)
    {
        response.ReplyMarkup = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                new InlineKeyboardButton(text)
                {
                    Url = url
                }
            }
        });
    }

    private static void RouteMultiple(MongoMessage message, ReverseSearchResponse response)
    {
        var aspectRatio = response.Photo.Width / (double)response.Photo.Height;
        aspectRatio = aspectRatio < 0.25 ? 0.25 : aspectRatio;
        aspectRatio = aspectRatio > 1.0 ? 1.0 : aspectRatio;

        var platformSizes = new Dictionary<FlufflePlatform, int>
        {
            { FlufflePlatform.FurAffinity, 79 },
            { FlufflePlatform.Twitter, 54 },
            { FlufflePlatform.E621, 36 },
            { FlufflePlatform.Weasyl, 51 },
            { FlufflePlatform.FurryNetwork, 100 }
        };

        var binOptions = new BinOption[]
        {
            new(1, (int)Math.Floor(275 * aspectRatio)),
            new(2, (int)Math.Floor(134 * aspectRatio)),
            new(3, (int)Math.Floor(90 * aspectRatio))
        };

        List<List<FluffleResult>> ComputeBins(FluffleResult[] results)
        {
            results = results.OrderByDescending(x => platformSizes[x.Platform]).ToArray();

            var bins = new List<List<FluffleResult>>();
            var index = 0;
            while (true)
            {
                var item = results[index];
                var binOption = binOptions
                    .Where(x => x.CompartmentSize >= platformSizes[item.Platform])
                    .OrderBy(x => x.CompartmentSize)
                    .First();

                var newIndex = index + Math.Min(binOption.BinSize, results.Length - index);
                bins.Add(results[index..newIndex].ToList());
                if (newIndex >= results.Length)
                    break;

                index = newIndex;
            }

            return bins;
        }

        List<List<FluffleResult>> bins;
        var results = message.FluffleResponse.Results.ToList();
        while (true)
        {
            bins = ComputeBins(results.ToArray());

            if (bins.Count <= MaxRows)
                break;

            results.Remove(results.MaxBy(x => x.Platform.Priority()));
        }

        while (true)
        {
            var hasChanges = false;
            for (var i = 0; i < bins.Count - 1; i++)
            {
                var above = bins[i];
                var below = bins[i + 1];

                if (above.Count <= below.Count)
                    continue;

                var item = above.Last();
                above.Remove(item);
                below.Insert(0, item);

                hasChanges = true;
            }

            if (!hasChanges)
                break;
        }

        var inlineKeyboardButtons = bins
            .Select(bin => bin
                .Select(x => new InlineKeyboardButton(x.Platform.Pretty()) { Url = x.Location })
                .ToList()
            ).ToList();
        response.ReplyMarkup = new InlineKeyboardMarkup(inlineKeyboardButtons);
    }
}

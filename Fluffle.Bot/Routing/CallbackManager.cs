using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Noppes.Fluffle.Bot.Routing;

public class CallbackManager
{
    public const int IdLength = 32;

    private readonly ITelegramRepository<CallbackContext, string> _repository;

    public CallbackManager(ITelegramRepository<CallbackContext, string> repository)
    {
        _repository = repository;
    }

    public async Task<InlineKeyboardMarkup> CreateAsync<T>(IList<(string name, T data)> options, string controller, string action)
    {
        var id = ShortUuid.Random(IdLength);
        await _repository.PutAsync(new CallbackContext
        {
            Id = id,
            CreatedAt = DateTime.UtcNow,
            ControllerName = controller,
            ControllerActionName = action,
            Options = options.Select(x => JsonSerializer.Serialize(x.data)).ToList()
        });

        var buttons = options
            .Select((x, i) => new List<InlineKeyboardButton> { new(x.name) { CallbackData = $"{id}-{i}" } })
            .ToList();

        return new InlineKeyboardMarkup(buttons);
    }

    public object Deserialize(string data, Type type) => JsonSerializer.Deserialize(data, type);
}

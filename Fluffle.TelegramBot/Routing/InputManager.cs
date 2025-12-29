using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fluffle.TelegramBot.Routing;

public class InputManager
{
    private readonly ITelegramRepository<InputContext, long> _repository;

    public InputManager(ITelegramRepository<InputContext, long> repository)
    {
        _repository = repository;
    }

    public async Task CreateAsync<T>(long chatId, T data, string controller, string action)
    {
        await _repository.PutAsync(new InputContext
        {
            Id = chatId,
            ControllerName = controller,
            ControllerActionName = action,
            CreatedAt = DateTime.UtcNow,
            Data = JsonSerializer.Serialize(data)
        });
    }
}

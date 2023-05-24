using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;

namespace Noppes.Fluffle.Bot.Routing;

public class InputAttribute : Attribute
{
}

public class CallbackContext
{
    public string Id { get; set; }

    public string ControllerName { get; set; }

    public string ControllerActionName { get; set; }

    public IList<string> Options { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class InputContext
{
    public long Id { get; set; }

    public string ControllerName { get; set; }

    public string ControllerActionName { get; set; }

    public string Data { get; set; }

    public DateTime CreatedAt { get; set; }
}

public interface ITelegramRepository<T, in TKey>
{
    Task PutAsync(T document);

    Task<T> GetAsync(TKey id);

    Task DeleteAsync(TKey id);
}

public sealed class UpdateAttribute : Attribute
{
    public UpdateType UpdateType { get; }

    public UpdateAttribute(UpdateType updateType)
    {
        UpdateType = updateType;
    }
}

public interface IUpdateHandler
{
    public Task HandleAsync(IServiceScope scope, Update update);
}

public interface ICallbackQueryHandler
{
    public Task HandleAsync(IServiceScope scope, CallbackQuery callbackQuery, string data);
}

public interface IInputHandler
{
    public Task HandleAsync(IServiceScope scope, Message message, string data);
}

public class FuncUpdateHandler : IUpdateHandler
{
    private readonly Func<Message, string> _func;

    public FuncUpdateHandler(Func<Message, string> func)
    {
        _func = func;
    }

    public async Task HandleAsync(IServiceScope scope, Update update)
    {
        var text = _func(update.EffectiveMessage());

        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
        await RateLimiter.RunAsync(update.EffectiveChat(), () => botClient.SendTextMessageAsync(update.EffectiveMessage().Chat.Id, text, ParseMode.MarkdownV2));
    }
}

public class ControllerUpdateHandler : IUpdateHandler
{
    private readonly ControllerAction _action;

    public ControllerUpdateHandler(ControllerAction action)
    {
        _action = action;
    }

    public Task HandleAsync(IServiceScope scope, Update update)
    {
        var controller = scope.ServiceProvider.GetRequiredService(_action.ControllerType);
        var result = _action.ActionInfo.Invoke(controller, new object[] { TelegramRouter.GetUpdateData(update) });

        return result is Task task ? task : Task.CompletedTask;
    }
}

public class ControllerInputHandler : IInputHandler
{
    private readonly ControllerAction _action;

    public ControllerInputHandler(ControllerAction action)
    {
        _action = action;
    }

    public Task HandleAsync(IServiceScope scope, Message message, string data)
    {
        var model = JsonSerializer.Deserialize(data, _action.Parameters.Last());

        var controller = scope.ServiceProvider.GetRequiredService(_action.ControllerType);
        var result = _action.ActionInfo.Invoke(controller, new[] { message, model });

        return result is Task task ? task : Task.CompletedTask;
    }
}

public class ControllerCallbackQueryHandler : ICallbackQueryHandler
{
    private readonly ControllerAction _action;

    public ControllerCallbackQueryHandler(ControllerAction action)
    {
        _action = action;
    }

    public Task HandleAsync(IServiceScope scope, CallbackQuery callbackQuery, string data)
    {
        var callbackManager = scope.ServiceProvider.GetRequiredService<CallbackManager>();
        var model = callbackManager.Deserialize(data, _action.Parameters.Last());

        var controller = scope.ServiceProvider.GetRequiredService(_action.ControllerType);
        var result = _action.ActionInfo.Invoke(controller, new[] { callbackQuery, model });

        return result is Task task ? task : Task.CompletedTask;
    }
}

public class ControllerAction
{
    public string Controller { get; set; }

    public Type ControllerType { get; set; }

    public string Action { get; set; }

    public MethodInfo ActionInfo { get; set; }

    public ICollection<Type> Parameters { get; set; }
}

public sealed class CommandAttribute : Attribute
{
    public string Command { get; set; }

    public CommandAttribute(string command)
    {
        Command = command;
    }
}

public sealed class CallbackQueryAttribute : Attribute
{
}

public interface IUpdateInterceptor
{
    public Task InterceptAsync(Update update);
}

public class TelegramRouter
{
    // Commands
    public readonly IDictionary<string, IUpdateHandler> CommandHandlers = new Dictionary<string, IUpdateHandler>(StringComparer.InvariantCultureIgnoreCase);

    // Callback queries
    public readonly IDictionary<(string, string), ICallbackQueryHandler> CallbackQueryHandlers = new Dictionary<(string, string), ICallbackQueryHandler>();

    // Input handler
    public readonly IDictionary<(string, string), IInputHandler> InputHandlers = new Dictionary<(string, string), IInputHandler>();

    public IDictionary<UpdateType, IUpdateHandler> DefaultHandlers = new Dictionary<UpdateType, IUpdateHandler>();

    public List<Type> Interceptors = new();

    private readonly IServiceProvider _services;
    private readonly ILogger<TelegramRouter> _logger;

    public TelegramRouter(IServiceProvider services, ILogger<TelegramRouter> logger)
    {
        _services = services;
        _logger = logger;
    }

    public void RegisterInterceptor<T>() where T : IUpdateInterceptor
    {
        Interceptors.Add(typeof(T));
    }

    public void RegisterController<T>()
    {
        var controllerName = typeof(T).Name;

        var actions = typeof(T).GetMethods()
            .Select(methodInfo => new ControllerAction
            {
                Controller = controllerName,
                ControllerType = typeof(T),
                Action = methodInfo.Name,
                ActionInfo = methodInfo,
                Parameters = methodInfo.GetParameters().Select(p => p.ParameterType).ToList()
            }).ToList();

        // Register controller command handlers
        foreach (var action in actions)
        {
            var command = action.ActionInfo.GetCustomAttribute<CommandAttribute>()?.Command;
            if (command == null)
                continue;

            if (action.Parameters.Count == 0)
                throw new InvalidOperationException($"Command handler {action.Controller}/{action.Action} does not have any parameters.");

            if (action.Parameters.Count > 1)
                throw new InvalidOperationException($"Command handler {action.Controller}/{action.Action} can only have one parameter.");

            if (action.Parameters.First() != typeof(Message))
                throw new InvalidOperationException($"Command handler {action.Controller}/{action.Action} its parameter must be of type {nameof(Message)}.");

            CommandHandlers.Add(command, new ControllerUpdateHandler(action));
        }

        // Register controller callback queries handler
        foreach (var action in actions)
        {
            var callbackQueryAttribute = action.ActionInfo.GetCustomAttribute<CallbackQueryAttribute>();
            if (callbackQueryAttribute == null)
                continue;

            if (action.Parameters.Count == 0)
                throw new InvalidOperationException($"Callback query handler {action.Controller}/{action.Action} does not have any parameters.");

            if (action.Parameters.Count > 2)
                throw new InvalidOperationException($"Callback query handler {action.Controller}/{action.Action} must have two parameters.");

            if (action.Parameters.First() != typeof(CallbackQuery))
                throw new InvalidOperationException($"Callback query handler {action.Controller}/{action.Action} its first parameter must be of type {nameof(CallbackQuery)}.");

            CallbackQueryHandlers.Add((action.Controller, action.Action), new ControllerCallbackQueryHandler(action));
        }

        // Register controller input handlers
        foreach (var action in actions)
        {
            var inputAttribute = action.ActionInfo.GetCustomAttribute<InputAttribute>();
            if (inputAttribute == null)
                continue;

            if (action.Parameters.Count == 0)
                throw new InvalidOperationException($"Input handler {action.Controller}/{action.Action} does not have any parameters.");

            if (action.Parameters.Count > 2)
                throw new InvalidOperationException($"Input handler {action.Controller}/{action.Action} must have exactly one parameter.");

            if (action.Parameters.First() != typeof(Message))
                throw new InvalidOperationException($"Input handler {action.Controller}/{action.Action} its parameter must be of type {nameof(Message)}.");

            InputHandlers.Add((action.Controller, action.Action), new ControllerInputHandler(action));
        }

        // Register default update handlers
        foreach (var action in actions)
        {
            var updateAttribute = action.ActionInfo.GetCustomAttribute<UpdateAttribute>();
            if (updateAttribute == null)
                continue;

            if (action.Parameters.Count == 0)
                throw new InvalidOperationException($"Update handler {action.Controller}/{action.Action} does not have any parameters.");

            if (action.Parameters.Count > 1)
                throw new InvalidOperationException($"Update handler {action.Controller}/{action.Action} can only have one parameter.");

            if (action.Parameters.First() != _updateDataTypes[updateAttribute.UpdateType])
                throw new InvalidOperationException($"Update handler {action.Controller}/{action.Action} its parameter must be of type {_updateDataTypes[updateAttribute.UpdateType].Name}.");

            DefaultHandlers.Add(updateAttribute.UpdateType, new ControllerUpdateHandler(action));
        }
    }

    public static dynamic GetUpdateData(Update update)
    {
        return update.Type switch
        {
            UpdateType.Message => update.Message,
            UpdateType.InlineQuery => update.InlineQuery,
            UpdateType.ChosenInlineResult => update.ChosenInlineResult,
            UpdateType.CallbackQuery => update.CallbackQuery,
            UpdateType.EditedMessage => update.EditedMessage,
            UpdateType.ChannelPost => update.ChannelPost,
            UpdateType.EditedChannelPost => update.EditedChannelPost,
            UpdateType.ShippingQuery => update.ShippingQuery,
            UpdateType.PreCheckoutQuery => update.PreCheckoutQuery,
            UpdateType.Poll => update.Poll,
            UpdateType.PollAnswer => update.PollAnswer,
            UpdateType.MyChatMember => update.MyChatMember,
            UpdateType.ChatMember => update.ChatMember,
            UpdateType.ChatJoinRequest => update.ChatJoinRequest,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private readonly IDictionary<UpdateType, Type> _updateDataTypes = new Dictionary<UpdateType, Type>
    {
        { UpdateType.Message, typeof(Message) },
        { UpdateType.InlineQuery, typeof(InlineQuery) },
        { UpdateType.ChosenInlineResult, typeof(ChosenInlineResult) },
        { UpdateType.CallbackQuery, typeof(CallbackQuery) },
        { UpdateType.EditedMessage, typeof(Message) },
        { UpdateType.ChannelPost, typeof(Message) },
        { UpdateType.EditedChannelPost, typeof(Message) },
        { UpdateType.ShippingQuery, typeof(ShippingQuery) },
        { UpdateType.PreCheckoutQuery, typeof(PreCheckoutQuery) },
        { UpdateType.Poll, typeof(Poll) },
        { UpdateType.PollAnswer, typeof(PollAnswer) },
        { UpdateType.MyChatMember, typeof(ChatMemberUpdated) },
        { UpdateType.ChatMember, typeof(ChatMemberUpdated) },
        { UpdateType.ChatJoinRequest, typeof(ChatJoinRequest) }
    };

    public async Task HandleUpdateAsync(Update update)
    {
        _logger.LogDebug("Dispatching worker to handle update with ID {id}.", update.Id);

        try
        {
            var worker = _services.GetRequiredService<TelegramRouterWorker>();
            await worker.HandleUpdateAsync(update);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred while executing a request.");
            throw;
        }
    }
}

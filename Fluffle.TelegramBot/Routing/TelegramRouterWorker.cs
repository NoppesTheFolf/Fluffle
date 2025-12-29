using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Fluffle.TelegramBot.Routing;

public class TelegramRouterWorker
{
    private readonly ITelegramBotClient _botClient;
    private readonly ITelegramRepository<CallbackContext, string> _callbackRepository;
    private readonly ITelegramRepository<InputContext, long> _inputRouteRepository;
    private readonly TelegramRouter _router;
    private readonly IServiceProvider _services;
    private readonly ILogger<TelegramRouterWorker> _logger;

    public TelegramRouterWorker(ITelegramBotClient botClient, ITelegramRepository<CallbackContext, string> callbackRepository, ITelegramRepository<InputContext, long> inputRouteRepository, TelegramRouter router, IServiceProvider services, ILogger<TelegramRouterWorker> logger)
    {
        _botClient = botClient;
        _callbackRepository = callbackRepository;
        _inputRouteRepository = inputRouteRepository;
        _router = router;
        _services = services;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(Update update)
    {
        Func<IServiceScope, Task> handleAsync = update.Type switch
        {
            UpdateType.CallbackQuery => scope => HandleCallbackAsync(scope, update.CallbackQuery),
            UpdateType.Message => scope =>
            {
                var message = update.Message!;

                if (message.Chat.Type is not ChatType.Private and not ChatType.Sender)
                    return HandleUpdateAsync(scope, update);

                if (string.IsNullOrWhiteSpace(message.Text))
                    return HandleUpdateAsync(scope, update);

                return message.Text.StartsWith('/')
                    ? HandleCommandAsync(scope, update)
                    : HandlePotentialInputAsync(scope, update);
            }
            ,
            _ => scope => HandleUpdateAsync(scope, update)
        };

        using var scope = _services.CreateScope();

        foreach (var interceptorType in _router.Interceptors)
        {
            var interceptor = (IUpdateInterceptor)scope.ServiceProvider.GetRequiredService(interceptorType);
            await interceptor.InterceptAsync(update);
        }

        await handleAsync(scope);
    }

    private async Task HandlePotentialInputAsync(IServiceScope scope, Update update)
    {
        var inputRoute = await _inputRouteRepository.GetAsync(update.Message!.Chat.Id);
        if (inputRoute == null || inputRoute.CreatedAt.Add(TimeSpan.FromHours(2)) < DateTime.UtcNow)
        {
            await HandleUpdateAsync(scope, update);
            return;
        }

        if (!_router.InputHandlers.TryGetValue((inputRoute.ControllerName, inputRoute.ControllerActionName), out var handler))
            return;

        await _inputRouteRepository.DeleteAsync(inputRoute.Id);
        await handler.HandleAsync(scope, update.Message, inputRoute.Data);
    }

    private async Task HandleCommandAsync(IServiceScope scope, Update update)
    {
        var commandParts = update.Message!.Text![1..]
            .Split(' ')
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        if (commandParts.Length == 0)
            return;

        var command = commandParts[0];
        if (!_router.CommandHandlers.TryGetValue(command, out var handler))
            return;

        await handler.HandleAsync(scope, update);
    }

    private async Task HandleUpdateAsync(IServiceScope scope, Update update)
    {
        if (!_router.DefaultHandlers.TryGetValue(update.Type, out var handler))
            return;

        await handler.HandleAsync(scope, update);
    }

    private const string NullData = "NULL";

    private async Task HandleCallbackAsync(IServiceScope scope, CallbackQuery callbackQuery)
    {
        // Skip callback query without any data
        if (string.IsNullOrWhiteSpace(callbackQuery.Data))
            return;

        // Skip callback query which has previously been answered
        if (callbackQuery.Data == NullData)
        {
            await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
            return;
        }

        // Callback queries should always exists in the {id}-{index} format. Check if the data
        // consists out of those two parts
        var callbackQueryDataParts = callbackQuery.Data.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (callbackQueryDataParts.Length != 2)
            return;

        // The ID will always have a exact length. Anything else is bollocks
        var id = callbackQueryDataParts[0];
        if (id.Length != CallbackManager.IdLength)
            return;

        // The index should be an integer
        if (!int.TryParse(callbackQueryDataParts[1], out var index))
            return;

        // Negative index are bollocks
        if (index < 0)
            return;

        // Get the callback batch from the callback repository. If it does not exists, then it
        // has probably been deleted because it is old
        var callbackBatch = await _callbackRepository.GetAsync(id);
        if (callbackBatch == null)
        {
            await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
            return;
        }

        // The index should not exceed the number of options defined in the callback batch
        if (index >= callbackBatch.Options.Count)
            return;

        // Callbacks older than two days cannot be answered. Allow for five minutes of processing time
        await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        if (DateTime.UtcNow.Subtract(callbackBatch.CreatedAt) > TimeSpan.FromDays(2).Subtract(TimeSpan.FromMinutes(5)))
            return;

        // Callback batches are not reusable. Delete it to save memory/storage
        await _callbackRepository.DeleteAsync(callbackBatch.Id);

        // Check which button the user clicked to generate this callback query
        var chosenOption = callbackQuery.Message!.ReplyMarkup!.InlineKeyboard
            .SelectMany(x => x)
            .FirstOrDefault(x => x.CallbackData == callbackQuery.Data);

        // Notify the user the button has been clicked by changing the inline keyboard to the selected option
        await RateLimiter.RunAsync(callbackQuery.Message.Chat, () => _botClient.EditMessageReplyMarkupAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, chosenOption == null ? InlineKeyboardMarkup.Empty() : new InlineKeyboardMarkup(new[] { new InlineKeyboardButton(chosenOption.Text) { CallbackData = NullData } })));

        // Finally we can let a controller handle the rest
        if (!_router.CallbackQueryHandlers.TryGetValue((callbackBatch.ControllerName, callbackBatch.ControllerActionName), out var handler))
            return;

        await handler.HandleAsync(scope, callbackQuery, callbackBatch.Options[index]);
    }
}

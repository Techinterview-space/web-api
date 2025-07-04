using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Services.Telegram;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Web.Api.Features.Telegram;

public abstract class TelegramBotHostedServiceBase<TChild, TBotProvider>
    where TBotProvider : ITelegramBotProvider
{
    protected ILogger<TChild> Logger { get; }

    private readonly TBotProvider _botClientProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly DateTime _startedToListenTo;

    protected TelegramBotHostedServiceBase(
        TBotProvider botClientProvider,
        ILogger<TChild> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _botClientProvider = botClientProvider;
        Logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _startedToListenTo = DateTime.UtcNow;
    }

    public void StartReceiving(
        CancellationToken cancellationToken)
    {
        var client = _botClientProvider.CreateClient();
        if (client is null)
        {
            return;
        }

        client.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandlePollingErrorAsync,
            receiverOptions: new ReceiverOptions
            {
                AllowedUpdates = GetUpdateTypes()
            },
            cancellationToken: cancellationToken);
    }

    protected abstract UpdateType[] GetUpdateTypes();

    protected abstract Task HandleUpdateAsync(
        IServiceScope scope,
        ITelegramBotClient client,
        Update updateRequest,
        CancellationToken cancellationToken);

    private Task HandlePollingErrorAsync(
        ITelegramBotClient client,
        Exception exception,
        CancellationToken cancellationToken)
    {
        Logger.LogError(
            exception,
            "An error occurred while polling: {Message}",
            exception.Message);

        return Task.CompletedTask;
    }

    private async Task HandleUpdateAsync(
        ITelegramBotClient client,
        Update updateRequest,
        CancellationToken cancellationToken)
    {
        Logger.LogDebug("Received update of type {UpdateType}", updateRequest.Type);

        var messageSent = updateRequest.Message?.Date;
        if (messageSent < _startedToListenTo)
        {
            Logger.LogWarning(
                "Ignoring message sent at {MessageSentDate} because it was sent before the bot started listening at {StartedToListenTo}",
                messageSent.Value.ToString("O"),
                _startedToListenTo.ToString("O"));

            return;
        }

        if (updateRequest.Message is null && updateRequest.InlineQuery is null && updateRequest.ChosenInlineResult is null)
        {
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();

        await HandleUpdateAsync(
            scope,
            client,
            updateRequest,
            cancellationToken);
    }
}
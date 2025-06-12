using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Services.Mediator;
using Infrastructure.Services.Telegram;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Web.Api.Features.Telegram.ProcessMessage;

namespace Web.Api.Features.Telegram;

public class TelegramBotHostedService
{
    private static readonly UpdateType[] _updateTypes = new List<UpdateType>
    {
        UpdateType.InlineQuery,
        UpdateType.Message,
        UpdateType.ChosenInlineResult,
    }.ToArray();

    private readonly ITelegramBotClientProvider _botClientProvider;
    private readonly ILogger<TelegramBotHostedService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly DateTime _startedToListenTo;

    public TelegramBotHostedService(
        ITelegramBotClientProvider botClientProvider,
        ILogger<TelegramBotHostedService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _botClientProvider = botClientProvider;
        _logger = logger;
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
                AllowedUpdates = _updateTypes
            },
            cancellationToken: cancellationToken);
    }

    private Task HandlePollingErrorAsync(
        ITelegramBotClient client,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
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
        _logger.LogDebug("Received update of type {UpdateType}", updateRequest.Type);
        if (updateRequest.Message is null && updateRequest.InlineQuery is null && updateRequest.ChosenInlineResult is null)
        {
            return;
        }

        var messageSent = updateRequest.Message?.Date;
        if (messageSent < _startedToListenTo)
        {
            _logger.LogWarning(
                "Ignoring message sent at {MessageSentDate} because it was sent before the bot started listening at {StartedToListenTo}",
                messageSent.Value.ToString("O"),
                _startedToListenTo.ToString("O"));

            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();

        await scope.ServiceProvider.HandleBy<ProcessTelegramMessageHandler, ProcessTelegramMessageCommand, string>(
            new ProcessTelegramMessageCommand(client, updateRequest),
            cancellationToken);
    }
}
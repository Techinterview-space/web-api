using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Web.Api.Features.Telegram.ProcessMessage;

namespace Web.Api.Features.Telegram;

public class TelegramBotService
{
    private static readonly UpdateType[] _updateTypes = new List<UpdateType>
    {
        UpdateType.InlineQuery,
        UpdateType.Message,
        UpdateType.ChosenInlineResult,
    }.ToArray();

    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramBotService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly DateTime _startedToListenTo;

    public TelegramBotService(
        IConfiguration configuration,
        ILogger<TelegramBotService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _startedToListenTo = DateTime.UtcNow;
    }

    public void StartReceiving(
        CancellationToken cancellationToken)
    {
        var enabled = _configuration["Telegram:Enable"]?.ToLowerInvariant();
        var parsedEnabled = bool.TryParse(enabled, out var isEnabled);

        var token = Environment.GetEnvironmentVariable("Telegram__BotToken");
        if (string.IsNullOrEmpty(token))
        {
            token = _configuration["Telegram:BotToken"];
        }

        if (!parsedEnabled ||
            !isEnabled ||
            string.IsNullOrEmpty(token))
        {
            _logger.LogWarning(
                "Telegram bot is disabled. Value {Value}. Parsed: {Parsed}",
                enabled,
                parsedEnabled);

            return;
        }

        var client = CreateClient(token);

        client.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
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
        _logger.LogError(exception, "An error occurred while polling: {Message}", exception.Message);
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
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(
            new ProcessTelegramMessageCommand(client, updateRequest),
            cancellationToken);
    }

    private TelegramBotClient CreateClient(
        string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new BadRequestException("Token is not set");
        }

        return new TelegramBotClient(token);
    }
}
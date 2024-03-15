using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Database;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Exceptions;
using Domain.Extensions;
using Domain.Salaries;
using Domain.Services.Global;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Domain.Telegram;

public class TelegramBotService
{
    private const string TgLineBreaker = "%0A";

    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramBotService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public TelegramBotService(
        IConfiguration configuration,
        ILogger<TelegramBotService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public void StartReceiving(
        CancellationToken cancellationToken)
    {
        var client = CreateClient();

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        ReceiverOptions receiverOptions = new ()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
        };

        client.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
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
        if (updateRequest.Message is null)
        {
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        var chatId = updateRequest.Message.Chat.Id;
        var message = updateRequest.Message;

        if (message.Entities?.Length > 0 && message.Entities[0].Type == MessageEntityType.Mention)
        {
            var requestParams = new TelegramMessageSalariesParams(message.Text);
            var salariesQuery = new SalariesForChartQuery(
                context,
                requestParams);

            var salaries = await salariesQuery
                .ToQueryable()
                .Select(x => x.Value)
                .ToListAsync(cancellationToken);

            var global = scope.ServiceProvider.GetRequiredService<IGlobal>();
            var frontendLink = new SalariesChartPageLink(global, requestParams);

            if (salaries.Count > 0)
            {
                var reply = "Специалисты ";
                if (requestParams.Grade.HasValue)
                {
                    reply += $" уровня {requestParams.Grade.Value}";
                }

                reply += $" получают в среднем {salaries.Average():N0} тг. {TgLineBreaker}Подробно на сайте " + frontendLink;
                await client.SendTextMessageAsync(chatId, reply, cancellationToken: cancellationToken);
                return;
            }

            await client.SendTextMessageAsync(chatId, "Нет информации о зарплатах =(", cancellationToken: cancellationToken);
            return;
        }

        await client.SendTextMessageAsync(chatId, "Неизвестная команда", cancellationToken: cancellationToken);
    }

    private TelegramBotClient CreateClient()
    {
        var token = Environment.GetEnvironmentVariable("Telegram__BotToken") ?? _configuration["Telegram:BotToken"];
        if (string.IsNullOrEmpty(token))
        {
            throw new BadRequestException("Token is not set");
        }

        return new TelegramBotClient(token);
    }
}
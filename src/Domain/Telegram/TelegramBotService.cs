using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Database;
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
using Telegram.Bot.Types.ReplyMarkups;

namespace Domain.Telegram;

public class TelegramBotService
{
    private const string TgLineBreaker = "\n";

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

        var messageText = message.Text ?? string.Empty;
        var mentionedInGroupChat =
            message.Entities?.Length > 0 &&
            message.Entities[0].Type == MessageEntityType.Mention &&
            messageText.StartsWith("@techinterview_salaries_bot");

        var privateMessage = message.Chat.Type == ChatType.Private;

        if (mentionedInGroupChat || privateMessage)
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
                var replyText = "Специалисты ";
                if (requestParams.Grade.HasValue)
                {
                    replyText += $" уровня {requestParams.Grade.Value}";
                }

                replyText += $" получают в среднем *{salaries.Median():N0} тг*. {TgLineBreaker}{TgLineBreaker}Подробно на сайте " + frontendLink;
                await client.SendTextMessageAsync(
                    chatId,
                    replyText,
                    parseMode: ParseMode.MarkdownV2,
                    replyMarkup: new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithUrl(
                            text: "Check sendMessage method",
                            url: frontendLink.ToString())),
                    cancellationToken: cancellationToken);
                return;
            }

            await client.SendTextMessageAsync(
                chatId,
                "Нет информации о зарплатах =(",
                cancellationToken: cancellationToken);
        }
    }

    private TelegramBotClient CreateClient()
    {
        var token = Environment.GetEnvironmentVariable("TelegramBotToken");
        if (string.IsNullOrEmpty(token))
        {
            token = _configuration["TelegramBotToken"];
        }

        Console.WriteLine(token);
        if (string.IsNullOrEmpty(token))
        {
            throw new BadRequestException("Token is not set");
        }

        return new TelegramBotClient(token);
    }
}
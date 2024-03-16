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
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace Domain.Telegram;

public class TelegramBotService
{
    private const string TelegramBotName = "@techinterview_salaries_bot";

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
        var enabled = _configuration["Telegram:Enabled"]?.ToLowerInvariant();
        var parsedEnabled = bool.TryParse(enabled, out var isEnabled);
        if (!parsedEnabled || !isEnabled)
        {
            _logger.LogWarning(
                "Telegram bot is disabled. Value {Value}. Parsed: {Parsed}", enabled, parsedEnabled);
            return;
        }

        var client = CreateClient();

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        ReceiverOptions receiverOptions = new ()
        {
            AllowedUpdates = new List<UpdateType>
            {
                UpdateType.InlineQuery,
                UpdateType.Message,
                UpdateType.ChosenInlineResult,
            }.ToArray()
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
        if (updateRequest.Message is null && updateRequest.InlineQuery is null && updateRequest.ChosenInlineResult is null)
        {
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        if (updateRequest.Type == UpdateType.InlineQuery && updateRequest.InlineQuery != null)
        {
            var results = new List<InlineQueryResult>();

            var counter = 0;

            var grades = new Dictionary<DeveloperGrade, string>()
            {
                { DeveloperGrade.Junior, "зарплата джуниоров" },
                { DeveloperGrade.Middle, "зарплата миддлов" },
                { DeveloperGrade.Senior, "зарплата сеньоров" },
                { DeveloperGrade.Lead, "зарплата лидов" },
            };

            results.Add(
                new InlineQueryResultArticle(
                    $"{counter}", // we use the counter as an id for inline query results
                    "Вся статистика по зарплатам", // inline query result title
                    new InputTextMessageContent($"{TelegramBotName} все зарплаты")));

            counter++;
            foreach (var grade in grades)
            {
                results.Add(new InlineQueryResultArticle(
                        $"{counter}", // we use the counter as an id for inline query results
                        grade.Key.ToString(), // inline query result title
                        new InputTextMessageContent($@"{TelegramBotName} {grade.Value}"))); // content that is submitted when the inline query result title is clicked

                counter++;
            }

            try
            {
                await client.AnswerInlineQueryAsync(
                    updateRequest.InlineQuery.Id,
                    results,
                    cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    "An error occurred while answering inline query: {Message}",
                    e.Message);
            }

            return;
        }

        if (updateRequest.Type == UpdateType.ChosenInlineResult && updateRequest.ChosenInlineResult != null)
        {
            var selectedGrade = updateRequest.ChosenInlineResult.ResultId.ToEnum<DeveloperGrade>();
            await ReplyWithSalariesAsync(
                client,
                updateRequest,
                new TelegramMessageSalariesParams(selectedGrade),
                context,
                scope,
                cancellationToken);

            return;
        }

        if (updateRequest.Message == null)
        {
            return;
        }

        var message = updateRequest.Message;
        var messageText = message.Text ?? string.Empty;
        var mentionedInGroupChat =
            message.Entities?.Length > 0 &&
            message.Entities[0].Type == MessageEntityType.Mention &&
            messageText.StartsWith(TelegramBotName);

        var privateMessage = message.Chat.Type == ChatType.Private;
        if (mentionedInGroupChat || privateMessage)
        {
            await ReplyWithSalariesAsync(
                client,
                updateRequest,
                new TelegramMessageSalariesParams(message.Text),
                context,
                scope,
                cancellationToken);
        }
    }

    private async Task ReplyWithSalariesAsync(
        ITelegramBotClient client,
        Update updateRequest,
        ISalariesChartQueryParams requestParams,
        DatabaseContext context,
        IServiceScope scope,
        CancellationToken cancellationToken)
    {
        var chatId = updateRequest.Message!.Chat.Id;
        var salariesQuery = new SalariesForChartQuery(
            context,
            requestParams);

        var totalCount = await salariesQuery.ToQueryable().CountAsync(cancellationToken);
        var salaries = await salariesQuery
            .ToQueryable(CompanyType.Local)
            .Select(x => x.Value)
            .ToListAsync(cancellationToken);

        var global = scope.ServiceProvider.GetRequiredService<IGlobal>();
        var frontendLink = new SalariesChartPageLink(global, requestParams);

        if (salaries.Count > 0)
        {
            var replyText = "В Казахстане специалисты IT ";
            if (requestParams.Grade.HasValue)
            {
                replyText += $" уровня {requestParams.Grade.Value}";
            }

            replyText += @$" получают в среднем <b>{salaries.Median():N0}</b> тг.

<em>Расчитано на основе {totalCount} анкет(ы)</em>
<em>Подробно на сайте <a href=""{frontendLink}"">techinterview.space/salaries</a></em>";
            await client.SendTextMessageAsync(
                chatId,
                replyText,
                parseMode: ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithUrl(
                        text: "techinterview.space/salaries",
                        url: frontendLink.ToString())),
                cancellationToken: cancellationToken);
            return;
        }

        await client.SendTextMessageAsync(
            chatId,
            "Нет информации о зарплатах =(",
            cancellationToken: cancellationToken);
    }

    private TelegramBotClient CreateClient()
    {
        var token = Environment.GetEnvironmentVariable("Telegram__BotToken");
        if (string.IsNullOrEmpty(token))
        {
            token = _configuration["Telegram:BotToken"];
        }

        Console.WriteLine(token);
        if (string.IsNullOrEmpty(token))
        {
            throw new BadRequestException("Token is not set");
        }

        return new TelegramBotClient(token);
    }
}
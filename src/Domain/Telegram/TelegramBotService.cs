using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Microsoft.Extensions.Caching.Memory;
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
    private const string CacheKey = "TelegramBotService_ReplyData";
    private const int CachingMinutes = 20;

    private static readonly Dictionary<DeveloperGrade, string> _gradeOptions = new Dictionary<DeveloperGrade, string>()
    {
        { DeveloperGrade.Junior, "зарплата джуниоров" },
        { DeveloperGrade.Middle, "зарплата миддлов" },
        { DeveloperGrade.Senior, "зарплата сеньоров" },
        { DeveloperGrade.Lead, "зарплата лидов" },
    };

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
        if (!parsedEnabled || !isEnabled)
        {
            _logger.LogWarning(
                "Telegram bot is disabled. Value {Value}. Parsed: {Parsed}", enabled, parsedEnabled);
            return;
        }

        var client = CreateClient();
        var receiverOptions = new ReceiverOptions()
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

        var messageSent = updateRequest.Message?.Date;
        if (messageSent < _startedToListenTo)
        {
            _logger.LogWarning(
                "Ignoring message sent at {MessageSentDate} because it was sent before the bot started listening at {StartedToListenTo}",
                messageSent.Value.ToString("O"),
                _startedToListenTo.ToString("O"));

            return;
        }

        if (updateRequest.Type == UpdateType.InlineQuery && updateRequest.InlineQuery != null)
        {
            await ProcessInlineQueryAsync(client, updateRequest, cancellationToken);
            return;
        }

        if (updateRequest.Message == null)
        {
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        var message = updateRequest.Message;
        var messageText = message.Text ?? string.Empty;
        var mentionedInGroupChat =
            message.Entities?.Length > 0 &&
            message.Entities[0].Type == MessageEntityType.Mention &&
            messageText.StartsWith(TelegramBotName);

        var privateMessage = message.Chat.Type == ChatType.Private;
        if (mentionedInGroupChat || privateMessage || Debugger.IsAttached)
        {
            var memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
            var parameters = new TelegramBotCommandParameters(message);
            var replyData = await memoryCache.GetOrCreateAsync(
                CacheKey + "_" + parameters.GetKeyPostfix(),
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CachingMinutes);
                    return await ReplyWithSalariesAsync(
                        new TelegramBotCommandParameters(message),
                        context,
                        scope,
                        cancellationToken);
                });

            var replyToMessageId = updateRequest.Message.ReplyToMessage?.MessageId ?? updateRequest.Message.MessageId;
            await client.SendTextMessageAsync(
                updateRequest.Message!.Chat.Id,
                replyData.ReplyText,
                parseMode: replyData.ParseMode,
                replyMarkup: replyData.InlineKeyboardMarkup,
                replyToMessageId: replyToMessageId,
                cancellationToken: cancellationToken);
        }
    }

    private async Task<TelegramBotReplyData> ReplyWithSalariesAsync(
        ISalariesChartQueryParams requestParams,
        DatabaseContext context,
        IServiceScope scope,
        CancellationToken cancellationToken)
    {
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

        const string frontendAppName = "techinterview.space/salaries";
        if (salaries.Count > 0 || Debugger.IsAttached)
        {
            var replyText = @$"<b>{salaries.Median():N0}</b> тг.

";
            replyText += @$"Столько специалисты в IT в Казахстане";
            if (requestParams.Grade.HasValue)
            {
                replyText += $" уровня <b>{requestParams.Grade.Value}</b>";
            }

            var maximumSalary = salaries.Count > 0 ? salaries.Max() : 0;
            replyText += @$" получают в среднем. Максимум: {maximumSalary:N0} тг.

<em>Расчитано на основе {totalCount} анкет(ы)</em>
<em>Подробно на сайте <a href=""{frontendLink}"">{frontendAppName}</a></em>";

            return new TelegramBotReplyData(
                replyText,
                new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithUrl(
                        text: frontendAppName,
                        url: frontendLink.ToString())));
        }

        return new TelegramBotReplyData(
            "Нет информации о зарплатах =(");
    }

    private async Task ProcessInlineQueryAsync(
        ITelegramBotClient client,
        Update updateRequest,
        CancellationToken cancellationToken)
    {
        var results = new List<InlineQueryResult>();

        var counter = 0;

        results.Add(
            new InlineQueryResultArticle(
                $"{counter}", // we use the counter as an id for inline query results
                "Вся статистика по зарплатам", // inline query result title
                new InputTextMessageContent($"{TelegramBotName} все зарплаты")));

        counter++;
        foreach (var grade in _gradeOptions)
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
                updateRequest.InlineQuery!.Id,
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
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
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
        var global = scope.ServiceProvider.GetRequiredService<IGlobal>();

        var allProfessions = await memoryCache.GetOrCreateAsync(
            CacheKey + "_AllProfessions",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(120);
                return await context
                    .Professions
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            });

        if (updateRequest.Type == UpdateType.InlineQuery && updateRequest.InlineQuery != null)
        {
            await ProcessInlineQueryAsync(
                client,
                context,
                memoryCache,
                global,
                allProfessions,
                updateRequest,
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
            var parameters = new TelegramBotCommandParameters(messageText, allProfessions);
            var replyData = await memoryCache.GetOrCreateAsync(
                CacheKey + "_" + parameters.GetKeyPostfix(),
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CachingMinutes);
                    return await ReplyWithSalariesAsync(
                        parameters,
                        context,
                        global,
                        null,
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
        TelegramBotCommandParameters requestParams,
        DatabaseContext context,
        IGlobal global,
        Profession professionOrNull,
        CancellationToken cancellationToken)
    {
        var salariesQuery = new SalariesForChartQuery(
            context,
            requestParams);

        var totalCount = await salariesQuery.ToQueryable().CountAsync(cancellationToken);
        var salaries = await salariesQuery
            .ToQueryable(CompanyType.Local)
            .Select(x => new
            {
                x.Grade,
                x.Value,
            })
            .ToListAsync(cancellationToken);

        var frontendLink = new SalariesChartPageLink(global, requestParams);

        const string frontendAppName = "techinterview.space/salaries";
        if (salaries.Count > 0 || Debugger.IsAttached)
        {
            var replyText = string.Empty;
            if (salaries.Count > 0)
            {
                var juniorMedian = salaries.Where(x => x.Grade == DeveloperGrade.Junior).Select(x => x.Value).Median();
                var middleMedian = salaries.Where(x => x.Grade == DeveloperGrade.Middle).Select(x => x.Value).Median();
                var seniorMedian = salaries.Where(x => x.Grade == DeveloperGrade.Senior).Select(x => x.Value).Median();
                var leadMedian = salaries.Where(x => x.Grade == DeveloperGrade.Lead).Select(x => x.Value).Median();

                if (juniorMedian > 0)
                {
                    replyText += @$"
Джуны: <b>{juniorMedian:N0}</b> тг.";
                }

                if (middleMedian > 0)
                {
                    replyText += @$"
Миддлы: <b>{middleMedian:N0}</b> тг.";
                }

                if (seniorMedian > 0)
                {
                    replyText += @$"
Сеньоры: <b>{seniorMedian:N0}</b> тг.";
                }

                if (leadMedian > 0)
                {
                    replyText += @$"
Лиды: <b>{leadMedian:N0}</b> тг.";
                }

                replyText += @$"Столько специалисты {professionOrNull?.Title ?? "в IT"} в Казахстане получают в среднем.

<em>Расчитано на основе {totalCount} анкет(ы)</em>
<em>Подробно на сайте <a href=""{frontendLink}"">{frontendAppName}</a></em>";
            }
            else
            {
                replyText = professionOrNull != null
                    ? $"Пока никто не оставил информацию о зарплатах для {professionOrNull.Title}."
                    : "Пока никто не оставлял информации о зарплатах.";

                replyText += @$"

<em>Посмотреть зарплаты по другим специальностям можно на сайте <a href=""{frontendLink}"">{frontendAppName}</a></em>";
            }

            return new TelegramBotReplyData(
                replyText.Trim(),
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
        DatabaseContext context,
        IMemoryCache memoryCache,
        IGlobal global,
        List<Profession> allProfessions,
        Update updateRequest,
        CancellationToken cancellationToken)
    {
        var results = new List<InlineQueryResult>();

        var counter = 0;

        var parametersForAllSalaries = new TelegramBotCommandParameters();
        var replyDataForAllSalaries = await memoryCache.GetOrCreateAsync(
            CacheKey + "_" + parametersForAllSalaries.GetKeyPostfix(),
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CachingMinutes);
                return await ReplyWithSalariesAsync(
                    parametersForAllSalaries,
                    context,
                    global,
                    null,
                    cancellationToken);
            });

        results.Add(
            new InlineQueryResultArticle(
                counter.ToString(),
                "Вся статистика без фильтра по специальности",
                new InputTextMessageContent(replyDataForAllSalaries.ReplyText)
                {
                    ParseMode = replyDataForAllSalaries.ParseMode,
                }));

        counter++;

        if (updateRequest.InlineQuery?.Query != null &&
            updateRequest.InlineQuery.Query.Length > 1)
        {
            foreach (var profession in allProfessions)
            {
                if (updateRequest.InlineQuery?.Query != null &&
                    !profession.Title.Contains(updateRequest.InlineQuery.Query, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var parameters = new TelegramBotCommandParameters(profession);

                var replyData = await memoryCache.GetOrCreateAsync(
                    CacheKey + "_" + parameters.GetKeyPostfix(),
                    async entry =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CachingMinutes);
                        return await ReplyWithSalariesAsync(
                            parameters,
                            context,
                            global,
                            profession,
                            cancellationToken);
                    });

                results.Add(new InlineQueryResultArticle(
                    counter.ToString(),
                    profession.Title,
                    new InputTextMessageContent(replyData.ReplyText)
                    {
                        ParseMode = replyData.ParseMode,
                    }));

                counter++;
            }
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
﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Entities.StatData;
using Domain.Entities.Telegram;
using Domain.Extensions;
using Infrastructure.Currencies.Contracts;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Infrastructure.Services.Global;
using Infrastructure.Services.Mediator;
using Infrastructure.Services.Professions;
using Infrastructure.Services.Telegram.ReplyMessages;
using Infrastructure.Services.Telegram.UserCommands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace Web.Api.Features.Telegram.ProcessSalariesRelatedMessage;

public class ProcessSalariesRelatedTelegramMessageHandler
    : IRequestHandler<ProcessTelegramMessageCommand, string>
{
    public const string SalariesPageUrl = "techinterview.space/salaries";
    private const string CacheKey = "TelegramBotService_ReplyData";

    private const int CachingMinutes = 20;

    private readonly ILogger<ProcessSalariesRelatedTelegramMessageHandler> _logger;
    private readonly ICurrencyService _currencyService;
    private readonly IProfessionsCacheService _professionsCacheService;
    private readonly DatabaseContext _context;
    private readonly IMemoryCache _cache;
    private readonly IGlobal _global;

    public ProcessSalariesRelatedTelegramMessageHandler(
        ILogger<ProcessSalariesRelatedTelegramMessageHandler> logger,
        ICurrencyService currencyService,
        DatabaseContext context,
        IMemoryCache cache,
        IGlobal global,
        IProfessionsCacheService professionsCacheService)
    {
        _logger = logger;
        _currencyService = currencyService;
        _context = context;
        _cache = cache;
        _global = global;
        _professionsCacheService = professionsCacheService;
    }

    public async Task<string> Handle(
        ProcessTelegramMessageCommand request,
        CancellationToken cancellationToken)
    {
        var allProfessions = await _professionsCacheService.GetProfessionsAsync(cancellationToken);

        if (request.UpdateRequest.Type == UpdateType.InlineQuery &&
            request.UpdateRequest.InlineQuery != null)
        {
            await ProcessInlineQueryAsync(
                request.BotClient,
                allProfessions,
                request.UpdateRequest,
                cancellationToken);

            return null;
        }

        if (request.UpdateRequest.Message == null)
        {
            return null;
        }

        var message = request.UpdateRequest.Message;
        var messageText = message.Text ?? string.Empty;

        var botUser = await GetBotUserName(request.BotClient);

        var messageSentByBot =
            message.ViaBot is not null &&
            message.ViaBot.Username == botUser.Username;

        if (messageSentByBot)
        {
            await AddInlineQueryClickAsync(
                message,
                cancellationToken);

            return null;
        }

        var mentionedInGroupChat =
            message.Entities?.Length > 0 &&
            message.Entities[0].Type == MessageEntityType.Mention &&
            botUser.Username != null &&
            messageText.StartsWith(botUser.Username);

        if (!mentionedInGroupChat &&
            message.Chat.Type is not ChatType.Private)
        {
            return null;
        }

        var directMessageResult = await TryProcessDirectMessageBotCommandAsync(
            request,
            cancellationToken);

        if (directMessageResult.Processed)
        {
            await IncrementTelegramBotUsageAsync(
                message,
                receivedMessageTextOrNull: messageText,
                usageType: TelegramBotUsageType.DirectMessage,
                cancellationToken: cancellationToken);

            return directMessageResult.ReplyText;
        }

        var parameters = TelegramBotUserCommandParameters.CreateFromMessage(
            messageText,
            allProfessions);

        var replyData = await ReplyWithSalariesAsync(
            message.Chat.Id.ToString(),
            parameters,
            cancellationToken);

        var replyToMessageId = request.UpdateRequest.Message.ReplyToMessage?.MessageId ?? request.UpdateRequest.Message.MessageId;
        await request.BotClient.SendMessage(
            message.Chat.Id,
            replyData.ReplyText,
            parseMode: replyData.ParseMode,
            replyParameters: new ReplyParameters
            {
                MessageId = replyToMessageId,
            },
            replyMarkup: replyData.InlineKeyboardMarkup,
            cancellationToken: cancellationToken);

        return replyData.ReplyText;
    }

    private async Task<(bool Processed, string ReplyText)> TryProcessDirectMessageBotCommandAsync(
        ProcessTelegramMessageCommand request,
        CancellationToken cancellationToken)
    {
        var message = request.UpdateRequest.Message!;
        var messageText = message.Text ?? string.Empty;

        if (message.Chat.Type is not ChatType.Private)
        {
            return (false, null);
        }

        if (messageText.Equals("/start", StringComparison.InvariantCultureIgnoreCase))
        {
            var startReplyData = new TelegramBotStartCommandReplyData(
                new ChartPageLink(
                    _global,
                    null)
                    .AddQueryParam("utm_source", message.Chat.Id.ToString())
                    .AddQueryParam("utm_campaign", "telegram-reply"));

            await request.BotClient.SendMessage(
                message.Chat.Id,
                startReplyData.ReplyText,
                parseMode: startReplyData.ParseMode,
                replyMarkup: startReplyData.InlineKeyboardMarkup,
                cancellationToken: cancellationToken);

            return (true, startReplyData.ReplyText);
        }

        if (messageText.Equals("/info", StringComparison.InvariantCultureIgnoreCase))
        {
            var replyMessage = $@"
Chat ID: {message.Chat.Id}
Тип чата: {message.Chat.Type}
Ваш айди: {message.From?.Id}
Никнейм: {message.From?.Username}
Имя: {message.From?.FirstName}
Фамилия: {message.From?.LastName}";

            await request.BotClient.SendMessage(
                message.Chat.Id,
                replyMessage,
                cancellationToken: cancellationToken);

            return (true, replyMessage);
        }

        if (messageText.Equals("/stats", StringComparison.InvariantCultureIgnoreCase))
        {
            var tgUserSettings = await _context.TelegramUserSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.ChatId == message.Chat.Id,
                    cancellationToken);

            TelegramBotReplyData replyMessage;
            if (tgUserSettings is null)
            {
                replyMessage = new CommandNotReadyReply();
            }
            else
            {
                replyMessage = await new StatsReplyMessageBuilder(_context)
                    .BuildAsync(cancellationToken);
            }

            await request.BotClient.SendMessage(
                message.Chat.Id,
                replyMessage.ReplyText,
                parseMode: replyMessage.ParseMode,
                cancellationToken: cancellationToken);

            return (true, replyMessage.ReplyText);
        }

        if (messageText.Equals("/help", StringComparison.InvariantCultureIgnoreCase))
        {
            var replyMessage = new HelpCommandMessageBuilder(_global)
                .Build();

            await request.BotClient.SendMessage(
                message.Chat.Id,
                replyMessage.ReplyText,
                parseMode: replyMessage.ParseMode,
                cancellationToken: cancellationToken);

            return (true, replyMessage.ReplyText);
        }

        return (false, null);
    }

    private async Task<TelegramBotReplyData> ReplyWithSalariesAsync(
        string utmSource,
        TelegramBotUserCommandParameters requestParams,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.Now;

        var (totalCount, salaries) = await _cache.GetOrCreateAsync(
            CacheKey + "_" + requestParams.GetKeyPostfix(),
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CachingMinutes);

                var salariesQuery = new SalariesForChartQuery(
                    _context,
                    requestParams,
                    now);

                var totalCount = await salariesQuery.CountAsync(cancellationToken);
                var salaries = await salariesQuery
                    .ToQueryable(CompanyType.Local)
                    .Where(x => x.Grade != null)
                    .Select(x => new SalaryBaseData
                    {
                        ProfessionId = x.ProfessionId,
                        Grade = x.Grade.Value,
                        Value = x.Value,
                        CreatedAt = x.CreatedAt,
                    })
                    .ToListAsync(cancellationToken);

                return (totalCount, salaries);
            });

        var salariesChartPageLink = new ChartPageLink(_global, requestParams)
            .AddQueryParam("utm_source", utmSource)
            .AddQueryParam("utm_campaign", "telegram-reply");

        var professions = requestParams.GetProfessionsTitleOrNull();

        string replyText;
        if (salaries.Count > 0)
        {
            var currencyContentOrNull = await _currencyService.GetCurrencyOrNullAsync(
                Currency.USD,
                cancellationToken);

            replyText = $"Зарплаты {professions ?? "специалистов IT в Казахстане"} по грейдам:\n";

            foreach (var gradeGroup in StatDataCacheItemSalaryData.GradeGroupsForRegularStats)
            {
                var median = salaries
                                .Where(x => x.Grade.GetGroupNameOrNull() == gradeGroup)
                                .Select(x => x.Value)
                                .Median();

                if (median > 0)
                {
                    var resultString = $"<b>{median.ToString("N0", CultureInfo.InvariantCulture)}</b> тг.";
                    if (currencyContentOrNull is not null)
                    {
                        resultString +=
                            $" (~{(median / currencyContentOrNull.Value).ToString("N0", CultureInfo.InvariantCulture)}{currencyContentOrNull.CurrencyString})";
                    }

                    replyText += $"\n{gradeGroup.ToCustomString()}: {resultString}";
                }
            }

            replyText += $"<em>\n\nРассчитано на основе {totalCount} анкет(ы)</em>" +
                $"\n<em>Разные графики и фильтры доступны по ссылке <a href=\"{salariesChartPageLink}\">{SalariesPageUrl}</a></em>" +
                $"\n\n#статистика_зарплат";
        }
        else
        {
            replyText = professions != null
                ? $"Пока никто не оставил информацию о зарплатах для {professions}."
                : "Пока никто не оставлял информации о зарплатах.";

            replyText += $"\n\n<em>Посмотреть зарплаты по другим специальностям можно " +
                $"на сайте <a href=\"{salariesChartPageLink}\">{SalariesPageUrl}</a></em>";
        }

        return new TelegramBotReplyData(
            replyText.Trim(),
            new InlineKeyboardMarkup(
                InlineKeyboardButton.WithUrl(
                    text: SalariesPageUrl,
                    url: salariesChartPageLink.ToString())));
    }

    private async Task ProcessInlineQueryAsync(
        ITelegramBotClient client,
        List<Profession> allProfessions,
        Update updateRequest,
        CancellationToken cancellationToken)
    {
        var results = new List<InlineQueryResult>();

        var counter = 0;

        var parametersForAllSalaries = new TelegramBotUserCommandParameters();
        var replyDataForAllSalaries = await ReplyWithSalariesAsync(
            "inline",
            parametersForAllSalaries,
            cancellationToken);

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
            var requestedProfession = updateRequest.InlineQuery.Query.ToLowerInvariant();
            if (ProductManagersTelegramBotUserCommandParameters.ShouldIncludeGroup(requestedProfession))
            {
                results.Add(
                    await GetProfessionsGroupInlineResultAsync(
                        counter,
                        new ProductManagersTelegramBotUserCommandParameters(allProfessions),
                        "Все продакты (Product managers)",
                        cancellationToken));

                counter++;
            }

            if (QaAndTestersTelegramBotUserCommandParameters.ShouldIncludeGroup(requestedProfession))
            {
                results.Add(
                    await GetProfessionsGroupInlineResultAsync(
                        counter,
                        new QaAndTestersTelegramBotUserCommandParameters(allProfessions),
                        "Тестировщики, QA и автоматизаторы",
                        cancellationToken));

                counter++;
            }

            foreach (var profession in allProfessions)
            {
                if (!profession.Title.Contains(requestedProfession, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var parameters = new TelegramBotUserCommandParameters(profession);

                var replyData = await ReplyWithSalariesAsync(
                    "inline",
                    parameters,
                    cancellationToken);

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
            await client.AnswerInlineQuery(
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

    private async Task AddInlineQueryClickAsync(
        Message message,
        CancellationToken cancellationToken)
    {
        if (message.From == null)
        {
            return;
        }

        var username = message.From.Username?.Trim() ?? message.From.Id.ToString();
        var chatId = message.Chat.Id;
        var chatName = message.Chat.Title?.Trim();

        _context.Add(
            new TelegramInlineReply(
                username,
                message.From.Id,
                chatId,
                chatName));

        await _context.TrySaveChangesAsync(cancellationToken);
    }

    private async Task<InlineQueryResultArticle> GetProfessionsGroupInlineResultAsync(
        int counter,
        TelegramBotUserCommandParameters professionGroupParams,
        string title,
        CancellationToken cancellationToken)
    {
        var professionsGroupReplyData = await ReplyWithSalariesAsync(
            "inline",
            professionGroupParams,
            cancellationToken);

        return new InlineQueryResultArticle(
            counter.ToString(),
            title,
            new InputTextMessageContent(professionsGroupReplyData.ReplyText)
            {
                ParseMode = professionsGroupReplyData.ParseMode,
            });
    }

    private async Task IncrementTelegramBotUsageAsync(
        Message message,
        string receivedMessageTextOrNull,
        TelegramBotUsageType usageType,
        CancellationToken cancellationToken)
    {
        var username = message.From?.Username ?? $"{message.From?.FirstName} {message.From?.LastName}".Trim();

        TelegramBotUsage usage = null;
        if (!string.IsNullOrEmpty(username))
        {
            usage = await _context
                .TelegramBotUsages
                .FirstOrDefaultAsync(
                    x => x.ChatId == message.Chat.Id,
                    cancellationToken);
        }

        if (usage == null)
        {
            usage = new TelegramBotUsage(
                message.Chat.Id,
                username,
                usageType);

            _context.TelegramBotUsages.Add(usage);
        }

        usage.IncrementUsageCount(receivedMessageTextOrNull);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<User> GetBotUserName(
        ITelegramBotClient telegramBotClient)
    {
        return await _cache.GetOrCreateAsync(
            CacheKey + "_BotUserName",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30);
                return await telegramBotClient.GetMe();
            });
    }
}
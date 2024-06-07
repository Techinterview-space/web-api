using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Entities.Telegram;
using Domain.Enums;
using Domain.Extensions;
using Infrastructure.Currencies.Contracts;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Infrastructure.Services.Global;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TechInterviewer.Features.Telegram.ProcessMessage.UserCommands;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace TechInterviewer.Features.Telegram.ProcessMessage;

public class ProcessTelegramMessageHandler : IRequestHandler<ProcessTelegramMessageCommand, Unit>
{
    public const string ApplicationName = "techinterview.space/salaries";

    private const string CacheKey = "TelegramBotService_ReplyData";

    private const int CachingMinutes = 20;

    private readonly ILogger<ProcessTelegramMessageHandler> _logger;
    private readonly ICurrencyService _currencyService;
    private readonly DatabaseContext _context;
    private readonly IMemoryCache _cache;
    private readonly IGlobal _global;

    public ProcessTelegramMessageHandler(
        ILogger<ProcessTelegramMessageHandler> logger,
        ICurrencyService currencyService,
        DatabaseContext context,
        IMemoryCache cache,
        IGlobal global)
    {
        _logger = logger;
        _currencyService = currencyService;
        _context = context;
        _cache = cache;
        _global = global;
    }

    private async Task<User> GetBotUserName(
        ITelegramBotClient telegramBotClient)
    {
        return await _cache.GetOrCreateAsync(
            CacheKey + "_BotUserName",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30);
                return await telegramBotClient.GetMeAsync();
            });
    }

    public async Task<Unit> Handle(
        ProcessTelegramMessageCommand request,
        CancellationToken cancellationToken)
    {
        var allProfessions = await _cache.GetOrCreateAsync(
            CacheKey + "_AllProfessions",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(120);
                return await _context
                    .Professions
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            });

        if (request.UpdateRequest.Type == UpdateType.InlineQuery &&
            request.UpdateRequest.InlineQuery != null)
        {
            await ProcessInlineQueryAsync(
                request.BotClient,
                allProfessions,
                request.UpdateRequest,
                cancellationToken);

            return Unit.Value;
        }

        if (request.UpdateRequest.Message == null)
        {
            return Unit.Value;
        }

        var message = request.UpdateRequest.Message;
        var messageText = message.Text ?? string.Empty;

        var botUser = await GetBotUserName(request.BotClient);
        var mentionedInGroupChat =
            message.Entities?.Length > 0 &&
            message.Entities[0].Type == MessageEntityType.Mention &&
            botUser.Username != null &&
            messageText.StartsWith(botUser.Username);

        var privateMessage = message.Chat.Type == ChatType.Private;
        if (mentionedInGroupChat || privateMessage)
        {
            if (privateMessage && messageText.Equals("/start", StringComparison.InvariantCultureIgnoreCase))
            {
                var startReplyData = new TelegramBotStartCommandReplyData(new SalariesChartPageLink(_global, null));
                await request.BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    startReplyData.ReplyText,
                    parseMode: startReplyData.ParseMode,
                    replyMarkup: startReplyData.InlineKeyboardMarkup,
                    cancellationToken: cancellationToken);

                return Unit.Value;
            }

            var parameters = TelegramBotUserCommandParameters.CreateFromMessage(
                messageText,
                allProfessions);

            var replyData = await _cache.GetOrCreateAsync(
                CacheKey + "_" + parameters.GetKeyPostfix(),
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CachingMinutes);
                    return await ReplyWithSalariesAsync(
                        parameters,
                        cancellationToken);
                });

            var replyToMessageId = request.UpdateRequest.Message.ReplyToMessage?.MessageId ?? request.UpdateRequest.Message.MessageId;
            await request.BotClient.SendTextMessageAsync(
                request.UpdateRequest.Message!.Chat.Id,
                replyData.ReplyText,
                parseMode: replyData.ParseMode,
                replyMarkup: replyData.InlineKeyboardMarkup,
                replyToMessageId: replyToMessageId,
                cancellationToken: cancellationToken);

            if (message.From! is not null)
            {
                var usageType = message.Chat.Type switch
                {
                    ChatType.Private => TelegramBotUsageType.DirectMessage,
                    ChatType.Sender => TelegramBotUsageType.DirectMessage,
                    ChatType.Group when mentionedInGroupChat => TelegramBotUsageType.GroupMention,
                    ChatType.Supergroup when mentionedInGroupChat => TelegramBotUsageType.SupergroupMention,
                    _ => TelegramBotUsageType.Undefined,
                };

                await GetOrCreateTelegramBotUsageAsync(
                    message.From.Username ?? $"{message.From.FirstName} {message.From.LastName}".Trim(),
                    message.Chat.Title ?? message.Chat.Username ?? message.Chat.Id.ToString(),
                    messageText,
                    usageType,
                    cancellationToken);
            }
        }

        return Unit.Value;
    }

    private async Task<TelegramBotReplyData> ReplyWithSalariesAsync(
        TelegramBotUserCommandParameters requestParams,
        CancellationToken cancellationToken)
    {
        var salariesQuery = new SalariesForChartQuery(
            _context,
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

        var frontendLink = new SalariesChartPageLink(_global, requestParams);
        var professions = requestParams.GetProfessionsTitleOrNull();

        string replyText;
        if (salaries.Count > 0)
        {
            var currencies = await _currencyService.GetCurrenciesAsync(
                [Currency.USD],
                cancellationToken);

            var gradeGroups = EnumHelper
                     .Values<GradeGroup>()
                     .Where(x => x is not(GradeGroup.Undefined or GradeGroup.Trainee));

            replyText = $"Зарплаты {professions ?? "специалистов IT в Казахстане"} по грейдам:\n";

            foreach (var gradeGroup in gradeGroups)
            {
                var median = salaries
                                .Where(x => x.Grade.GetGroupNameOrNull() == gradeGroup)
                                .Select(x => x.Value)
                                .Median();

                if (median > 0)
                {
                    var resStr = $"<b>{median:N0}</b> тг.";
                    foreach (var currencyContent in currencies)
                    {
                        resStr += $" (~{median / currencyContent.Value:N0}{currencyContent.CurrencyString})";
                    }

                    replyText += $"\n{gradeGroup.ToCustomString()}: {resStr}";
                }
            }

            replyText += $"<em>\n\nРасчитано на основе {totalCount} анкет(ы)</em>" +
                $"\n<em>Подробно на сайте <a href=\"{frontendLink}\">{ApplicationName}</a></em>";
        }
        else
        {
            replyText = professions != null
                ? $"Пока никто не оставил информацию о зарплатах для {professions}."
                : "Пока никто не оставлял информации о зарплатах.";

            replyText += $"\n\n<em>Посмотреть зарплаты по другим специальностям можно " +
                $"на сайте <a href=\"{frontendLink}\">{ApplicationName}</a></em>";
        }

        return new TelegramBotReplyData(
            replyText.Trim(),
            new InlineKeyboardMarkup(
                InlineKeyboardButton.WithUrl(
                    text: ApplicationName,
                    url: frontendLink.ToString())));
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
        var replyDataForAllSalaries = await _cache.GetOrCreateAsync(
            CacheKey + "_" + parametersForAllSalaries.GetKeyPostfix(),
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CachingMinutes);
                return await ReplyWithSalariesAsync(
                    parametersForAllSalaries,
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

                var replyData = await _cache.GetOrCreateAsync(
                    CacheKey + "_" + parameters.GetKeyPostfix(),
                    async entry =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CachingMinutes);
                        return await ReplyWithSalariesAsync(
                            parameters,
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

            var userName = updateRequest.InlineQuery.From.Username
                           ?? $"{updateRequest.InlineQuery.From.FirstName} {updateRequest.InlineQuery.From.LastName}".Trim();
            await GetOrCreateTelegramBotUsageAsync(
                userName,
                null,
                updateRequest.InlineQuery?.Query,
                TelegramBotUsageType.InlineQuery,
                cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "An error occurred while answering inline query: {Message}",
                e.Message);
        }
    }

    private async Task<InlineQueryResultArticle> GetProfessionsGroupInlineResultAsync(
        int counter,
        TelegramBotUserCommandParameters professionGroupParams,
        string title,
        CancellationToken cancellationToken)
    {
        var professionsGroupReplyData = await _cache.GetOrCreateAsync(
            CacheKey + "_" + professionGroupParams.GetKeyPostfix(),
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CachingMinutes);
                return await ReplyWithSalariesAsync(
                    professionGroupParams,
                    cancellationToken);
            });

        return new InlineQueryResultArticle(
            counter.ToString(),
            title,
            new InputTextMessageContent(professionsGroupReplyData.ReplyText)
            {
                ParseMode = professionsGroupReplyData.ParseMode,
            });
    }

    private async Task GetOrCreateTelegramBotUsageAsync(
        string username,
        string channelName,
        string receivedMessageTextOrNull,
        TelegramBotUsageType usageType,
        CancellationToken cancellationToken)
    {
        var usage = await _context
            .TelegramBotUsages
            .FirstOrDefaultAsync(
                x => x.Username == username && x.UsageType == usageType,
                cancellationToken);

        if (usage == null)
        {
            usage = new TelegramBotUsage(username, channelName, usageType);
            _context.TelegramBotUsages.Add(usage);
        }

        usage.IncrementUsageCount(receivedMessageTextOrNull);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Entities.Telegram;
using Domain.Extensions;
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

    private const string TelegramBotName = "@techinterview_salaries_bot";
    private const string CacheKey = "TelegramBotService_ReplyData";

    private const int CachingMinutes = 20;

    private readonly ILogger<ProcessTelegramMessageHandler> _logger;
    private readonly DatabaseContext _context;
    private readonly IMemoryCache _cache;
    private readonly IGlobal _global;

    public ProcessTelegramMessageHandler(
        ILogger<ProcessTelegramMessageHandler> logger,
        DatabaseContext context,
        IMemoryCache cache,
        IGlobal global)
    {
        _logger = logger;
        _context = context;
        _cache = cache;
        _global = global;
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

        if (request.UpdateRequest.Type == UpdateType.InlineQuery && request.UpdateRequest.InlineQuery != null)
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
        var mentionedInGroupChat =
            message.Entities?.Length > 0 &&
            message.Entities[0].Type == MessageEntityType.Mention &&
            messageText.StartsWith(TelegramBotName);

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

        if (salaries.Count > 0 || Debugger.IsAttached)
        {
            string replyText;
            if (salaries.Count > 0)
            {
                var juniorMedian = salaries.Where(x => x.Grade == DeveloperGrade.Junior).Select(x => x.Value).Median();
                var middleMedian = salaries.Where(x => x.Grade == DeveloperGrade.Middle).Select(x => x.Value).Median();
                var seniorMedian = salaries.Where(x => x.Grade == DeveloperGrade.Senior).Select(x => x.Value).Median();
                var leadMedian = salaries.Where(x => x.Grade == DeveloperGrade.Lead).Select(x => x.Value).Median();

                replyText = @$"Зарплаты {professions ?? "специалистов IT в Казахстане"} по грейдам:
";

                if (juniorMedian > 0)
                {
                    replyText += @$"
Джуны:   <b>{juniorMedian:N0}</b> тг.";
                }

                if (middleMedian > 0)
                {
                    replyText += @$"
Миддлы:  <b>{middleMedian:N0}</b> тг.";
                }

                if (seniorMedian > 0)
                {
                    replyText += @$"
Сеньоры: <b>{seniorMedian:N0}</b> тг.";
                }

                if (leadMedian > 0)
                {
                    replyText += @$"
Лиды:    <b>{leadMedian:N0}</b> тг.";
                }

                replyText += @$"

<em>Расчитано на основе {totalCount} анкет(ы)</em>
<em>Подробно на сайте <a href=""{frontendLink}"">{ApplicationName}</a></em>";
            }
            else
            {
                replyText = professions != null
                    ? $"Пока никто не оставил информацию о зарплатах для {professions}."
                    : "Пока никто не оставлял информации о зарплатах.";

                replyText += @$"

<em>Посмотреть зарплаты по другим специальностям можно на сайте <a href=""{frontendLink}"">{ApplicationName}</a></em>";
            }

            return new TelegramBotReplyData(
                replyText.Trim(),
                new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithUrl(
                        text: ApplicationName,
                        url: frontendLink.ToString())));
        }

        return new TelegramBotReplyData(
            "Нет информации о зарплатах =(");
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
            if (requestedProfession.Equals(
                    ProductManagersTelegramBotUserCommandParameters.ProductProfessionTitle,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                var productProfessionParams = new ProductManagersTelegramBotUserCommandParameters(allProfessions);
                var productReplyData = await _cache.GetOrCreateAsync(
                    CacheKey + "_" + productProfessionParams.GetKeyPostfix(),
                    async entry =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CachingMinutes);
                        return await ReplyWithSalariesAsync(
                            productProfessionParams,
                            cancellationToken);
                    });

                results.Add(
                    new InlineQueryResultArticle(
                        counter.ToString(),
                        "Все продакты (Product managers)",
                        new InputTextMessageContent(productReplyData.ReplyText)
                        {
                            ParseMode = productReplyData.ParseMode,
                        }));

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
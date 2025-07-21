using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Entities.StatData;
using Domain.Entities.StatData.Salary;
using Domain.Entities.Telegram;
using Domain.Extensions;
using Domain.ValueObjects;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using Web.Api.Features.Salaries.GetSalariesChart.Charts;

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
    private readonly IConfiguration _configuration;

    public ProcessSalariesRelatedTelegramMessageHandler(
        ILogger<ProcessSalariesRelatedTelegramMessageHandler> logger,
        ICurrencyService currencyService,
        DatabaseContext context,
        IMemoryCache cache,
        IGlobal global,
        IProfessionsCacheService professionsCacheService,
        IConfiguration configuration)
    {
        _logger = logger;
        _currencyService = currencyService;
        _context = context;
        _cache = cache;
        _global = global;
        _professionsCacheService = professionsCacheService;
        _configuration = configuration;
    }

    public async Task<string> Handle(
        ProcessTelegramMessageCommand request,
        CancellationToken cancellationToken)
    {
        if (request.UpdateRequest.ChosenInlineResult is not null)
        {
            _logger.LogInformation(
                "TELEGRAM_BOT. Salaries. Processing ChosenInlineResult " +
                "with InlineMessageId: {InlineMessageId} " +
                "from {Name}. " +
                "Id {Id}. " +
                "Query {Query}. " +
                "IsBot {IsBot}",
                request.UpdateRequest.ChosenInlineResult.InlineMessageId,
                request.UpdateRequest.ChosenInlineResult.From.Username,
                request.UpdateRequest.ChosenInlineResult.From.Id,
                request.UpdateRequest.ChosenInlineResult.Query,
                request.UpdateRequest.ChosenInlineResult.From.IsBot);

            await _context.SaveAsync(
                new TelegramInlineReply(
                    request.UpdateRequest.ChosenInlineResult.From.Id,
                    request.UpdateRequest.ChosenInlineResult.Query,
                    TelegramBotType.Salaries),
                cancellationToken);

            return null;
        }

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
            await _context.SaveAsync(
                new TelegramInlineReply(
                    message.Chat.Id,
                    message.Text,
                    TelegramBotType.Salaries),
                cancellationToken);

            return null;
        }

        var replyToMessageId = request.UpdateRequest.Message.ReplyToMessage?.MessageId ?? request.UpdateRequest.Message.MessageId;

        var hasJopPostingInfo = message.Entities?.Length > 0 &&
                                message.Entities.Any(x => x.Type is MessageEntityType.Hashtag) &&
                                message.EntityValues?.Any(x => x.ToLowerInvariant() == "#вакансия") == true &&
                                (message.Chat.Type is ChatType.Supergroup or ChatType.Group);

        _logger.LogInformation(
            "techinterview_space_bot. Processing job posting message from {Name}. " +
            "Id {Id}. " +
            "Type {Type}. " +
            "ReplyTo {replyToMessageId}. " +
            "Has Job info {HasJobInfo}. " +
            "Text: {Text}",
            message.Chat.Username,
            message.Chat.Id,
            message.Chat.Type.ToString(),
            replyToMessageId,
            hasJopPostingInfo,
            message.Text);

        if (hasJopPostingInfo)
        {
            // If the message is a bot command, we will process it separately.
            var jobSalaryInfo = new JobPostingParser(message.Text).GetResult();
            if (!jobSalaryInfo.HasAnySalary())
            {
                return null;
            }

            var jobPostingSubscription = await _context.JobPostingMessageSubscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TelegramChatId == message.Chat.Id,
                    cancellationToken);

            if (jobPostingSubscription is null)
            {
                return null;
            }

            var jobPostingProfessions = allProfessions
                .Where(x => jobPostingSubscription.ProfessionIds.Contains(x.Id))
                .ToList();

            var salariesForReply = await new SalariesForChartQuery(
                    _context,
                    new TelegramBotUserCommandParameters(jobPostingProfessions),
                    DateTimeOffset.UtcNow)
                .Where(x => x.Company == CompanyType.Local)
                .Where(x => x.Grade != null)
                .ToQueryable(x => new SalaryBaseData
                {
                    Grade = x.Grade.Value,
                    Value = x.Value,
                })
                .ToListAsync(cancellationToken);

            if (salariesForReply.Count == 0)
            {
                _logger.LogInformation(
                    "No salaries found for JobPosting subscription ID {JobPostingMessageSubscriptionId} in job posting reply.",
                    jobPostingSubscription.Id);

                return null;
            }

            var textToSend =
                new SalaryGradeRanges(salariesForReply, jobPostingProfessions).ToTelegramHtml(
                    jobSalaryInfo.MinSalary,
                    jobSalaryInfo.MaxSalary);

            if (textToSend is not null)
            {
                await request.BotClient.SendMessage(
                    message.Chat.Id,
                    textToSend,
                    parseMode: ParseMode.Html,
                    replyParameters: new ReplyParameters
                    {
                        MessageId = replyToMessageId,
                    },
                    linkPreviewOptions: new LinkPreviewOptions
                    {
                        IsDisabled = true,
                    },
                    cancellationToken: cancellationToken);
            }
            else
            {
                _logger.LogInformation(
                    "Empty reply text for subscription {JobPostingMessageSubscriptionId} in job posting reply.",
                    jobPostingSubscription.Id);
            }

            return textToSend;
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

        var directMessageResult = await TryProcessBotCommandAsync(
            request,
            cancellationToken);

        if (directMessageResult.Processed)
        {
            await SaveBotMessageAsync(
                message,
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

        await SaveBotMessageAsync(
            message,
            usageType: TelegramBotUsageType.DirectMessage,
            cancellationToken: cancellationToken);

        return replyData.ReplyText;
    }

    private async Task<(bool Processed, string ReplyText)> TryProcessBotCommandAsync(
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
                new SalariesChartPageLink(
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

        var salariesChartPageLink = new SalariesChartPageLink(_global, requestParams)
            .AddQueryParam("utm_source", utmSource)
            .AddQueryParam("utm_campaign", "telegram-reply");

        var professions = requestParams.GetProfessionsTitleOrNull();

        string replyText;
        if (salaries.Count > 0)
        {
            var currencyContentOrNull = await _currencyService.GetCurrencyOrNullAsync(
                Currency.USD,
                cancellationToken);

            replyText = $"Зарплаты {professions ?? "специалистов IT в Казахстане"} по грейдам (медиана):\n";

            foreach (var gradeGroup in SalariesStatDataCacheItemSalaryData.GradeGroupsForRegularStats)
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
                cacheTime: 10 * 60,
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

    private async Task SaveBotMessageAsync(
        Message message,
        TelegramBotUsageType usageType,
        CancellationToken cancellationToken)
    {
        var adminUsername = _configuration["Telegram:AdminUsername"];
        var username = message.From?.Username?.Trim().ToLowerInvariant() ??
                       message.From?.FirstName?.Trim().ToLowerInvariant() ??
                       message.From?.LastName?.Trim().ToLowerInvariant() ??
                       "unknown";

        await _context.SaveAsync(
            new SalariesBotMessage(
                message.Chat.Id,
                username,
                usageType,
                username == adminUsername),
            cancellationToken);
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
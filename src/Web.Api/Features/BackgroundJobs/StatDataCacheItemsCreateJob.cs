using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Entities.StatData;
using Domain.Enums;
using Domain.Extensions;
using Infrastructure.Currencies.Contracts;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Infrastructure.Services.Global;
using Infrastructure.Services.Professions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Web.Api.Features.Telegram;
using Web.Api.Features.Telegram.ProcessMessage;
using Web.Api.Features.Telegram.ProcessMessage.UserCommands;

namespace Web.Api.Features.BackgroundJobs;

public class StatDataCacheItemsCreateJob
    : InvocableJobBase<StatDataCacheItemsCreateJob>
{
    public const string SalariesPageUrl = "techinterview.space/salaries";

    private readonly DatabaseContext _context;
    private readonly ICurrencyService _currencyService;
    private readonly IProfessionsCacheService _professionsCacheService;
    private readonly IGlobal _global;
    private readonly TelegramBotClientProvider _botClientProvider;

    public StatDataCacheItemsCreateJob(
        ILogger<StatDataCacheItemsCreateJob> logger,
        DatabaseContext context,
        ICurrencyService currencyService,
        IGlobal global,
        IProfessionsCacheService professionsCacheService,
        TelegramBotClientProvider botClientProvider)
        : base(logger)
    {
        _context = context;
        _currencyService = currencyService;
        _global = global;
        _professionsCacheService = professionsCacheService;
        _botClientProvider = botClientProvider;
    }

    public override async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var cacheRecords = await _context.StatDataCacheRecords
            .ToListAsync(cancellationToken);

        if (cacheRecords.Count == 0)
        {
            Logger.LogInformation(
                "No StatDataCache records found. Exiting job.");
        }

        var allProfessions = await _professionsCacheService.GetProfessionsAsync(cancellationToken);
        var currencies = await _currencyService.GetCurrenciesAsync(
            [Currency.USD],
            cancellationToken);

        var listOfDataToBeSent = new List<(StatDataCacheItem Item, TelegramBotReplyData Data)>();
        var now = DateTimeOffset.Now;

        foreach (var statDataCache in cacheRecords)
        {
            var lastCacheItemOrNull = await _context.StatDataCacheItems
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var filterData = new TelegramBotUserCommandParameters(
                allProfessions
                    .Where(x => statDataCache.ProfessionIds.Contains(x.Id))
                    .ToList());

            var salariesQuery = new SalariesForChartQuery(
                _context,
                filterData,
                now);

            var totalCount = await salariesQuery.CountAsync(cancellationToken);
            var salaries = await salariesQuery
                .ToQueryable(CompanyType.Local)
                .Select(x => new
                {
                    x.Grade,
                    x.Value,
                })
                .ToListAsync(cancellationToken);

            var salariesChartPageLink = new ChartPageLink(_global, filterData);

            var gradeGroups = EnumHelper
                .Values<GradeGroup>()
                .Where(x => x is not(GradeGroup.Undefined or GradeGroup.Trainee));

            var professions = filterData.GetProfessionsTitleOrNull();

            string textMessageToBeSent;
            if (salaries.Count > 0)
            {
                textMessageToBeSent = $"Зарплаты {professions ?? "специалистов IT в Казахстане"} по грейдам:\n";

                foreach (var gradeGroup in gradeGroups)
                {
                    var median = salaries
                        .Where(x => x.Grade.GetGroupNameOrNull() == gradeGroup)
                        .Select(x => x.Value)
                        .Median();

                    if (median > 0)
                    {
                        var resStr = $"<b>{median.ToString("N0", CultureInfo.InvariantCulture)}</b> тг.";
                        foreach (var currencyContent in currencies)
                        {
                            resStr += $" (~{(median / currencyContent.Value).ToString("N0", CultureInfo.InvariantCulture)}{currencyContent.CurrencyString})";
                        }

                        textMessageToBeSent += $"\n{gradeGroup.ToCustomString()}: {resStr}";
                    }
                }

                textMessageToBeSent += $"<em>\n\nРассчитано на основе {totalCount} анкет(ы)</em>" +
                             $"\n<em>Подробно на сайте <a href=\"{salariesChartPageLink}\">{SalariesPageUrl}</a></em>";
            }
            else
            {
                textMessageToBeSent = professions != null
                    ? $"Пока никто не оставил информацию о зарплатах для {professions}."
                    : "Пока никто не оставлял информации о зарплатах.";

                textMessageToBeSent += $"\n\n<em>Посмотреть зарплаты по другим специальностям можно " +
                             $"на сайте <a href=\"{salariesChartPageLink}\">{SalariesPageUrl}</a></em>";
            }

            var dataTobeSent = new TelegramBotReplyData(
                textMessageToBeSent.Trim(),
                new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithUrl(
                        text: SalariesPageUrl,
                        url: salariesChartPageLink.ToString())));

            var cacheItem = new StatDataCacheItem(
                statDataCache,
                lastCacheItemOrNull,
                new StatDataCacheItemSalaryData(
                    salaries
                        .Select(x => x.Value)
                        .ToList(),
                    totalCount));

            _context.Add(cacheItem);
            listOfDataToBeSent.Add((cacheItem, dataTobeSent));
        }

        await _context.TrySaveChangesAsync(cancellationToken);

        var client = _botClientProvider.CreateClient();
        if (client is null)
        {
            Logger.LogWarning("Telegram bot is disabled.");
            return;
        }

        var failedToSend = new List<(StatDataCacheItem CacheItem, Exception Ex)>();

        foreach (var data in listOfDataToBeSent)
        {
            try
            {
                await client.SendTextMessageAsync(
                    data.Item.GetChatId(),
                    data.Data.ReplyText,
                    parseMode: data.Data.ParseMode,
                    replyMarkup: data.Data.InlineKeyboardMarkup,
                    cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                failedToSend.Add((data.Item, e));
            }
        }

        if (failedToSend.Count > 0)
        {
            Logger.LogError(
                "Failed to send regular stats updates chats to {Count} users. Errors: {Errors}",
                failedToSend.Count,
                failedToSend.Select(x => x.Ex.Message + ", " + x.Ex.GetType().FullName));
        }
    }
}
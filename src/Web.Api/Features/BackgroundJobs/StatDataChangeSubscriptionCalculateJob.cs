using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Entities.StatData;
using Domain.Extensions;
using Infrastructure.Currencies.Contracts;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Infrastructure.Services.Global;
using Infrastructure.Services.Professions;
using Infrastructure.Services.Telegram.ReplyMessages;
using Infrastructure.Services.Telegram.UserCommands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;
using Web.Api.Features.BackgroundJobs.Models;
using Web.Api.Features.Telegram;

namespace Web.Api.Features.BackgroundJobs;

public class StatDataChangeSubscriptionCalculateJob
    : InvocableJobBase<StatDataChangeSubscriptionCalculateJob>
{
    public const string SalariesPageUrl = "techinterview.space/salaries";

    private readonly DatabaseContext _context;
    private readonly ICurrencyService _currencyService;
    private readonly IProfessionsCacheService _professionsCacheService;
    private readonly IGlobal _global;
    private readonly TelegramBotClientProvider _botClientProvider;

    public StatDataChangeSubscriptionCalculateJob(
        ILogger<StatDataChangeSubscriptionCalculateJob> logger,
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
        var subscriptions = await _context.StatDataChangeSubscriptions
            .Include(x => x.AiAnalysisRecords)
            .Where(x => x.DeletedAt == null)
            .ToListAsync(cancellationToken);

        if (subscriptions.Count == 0)
        {
            Logger.LogInformation(
                "No StatDataCache records found. Exiting job.");
        }

        var allProfessions = await _professionsCacheService.GetProfessionsAsync(cancellationToken);
        var usdCurrencyOrNull = await _currencyService.GetCurrencyOrNullAsync(
            Currency.USD,
            cancellationToken);

        var listOfDataToBeSent = new List<(StatDataChangeSubscriptionRecord Item, TelegramBotReplyData Data)>();
        var now = DateTimeOffset.Now;

        foreach (var subscription in subscriptions)
        {
            var subscriptionData = await new SalarySubscriptionData(
                    allProfessions: allProfessions,
                    subscription: subscription,
                    context: _context,
                    now: now)
                .InitializeAsync(cancellationToken);

            var lastCacheItemOrNull = subscriptionData.LastCacheItemOrNull;
            if (subscriptionData.Salaries.Count == 0)
            {
                Logger.LogInformation(
                    "No salaries found for subscription {SubscriptionId} ({Name}). Skipping notification.",
                    subscription.Id,
                    subscription.Name);

                continue;
            }

            var professions = subscriptionData.FilterData.GetProfessionsTitleOrNull();
            var textMessageToBeSent = $"Зарплаты {professions ?? "специалистов IT в Казахстане"} по грейдам на дату {now:yyyy-MM-dd}:\n\n";

            var hasAnyDifference = lastCacheItemOrNull == null;
            foreach (var gradeGroup in StatDataCacheItemSalaryData.GradeGroupsForRegularStats)
            {
                var median = subscriptionData.Salaries
                    .Where(x => x.Grade.GetGroupNameOrNull() == gradeGroup)
                    .Select(x => x.Value)
                    .Median();

                var line = $"{gradeGroup.ToCustomString()}: ";
                if (median <= 0)
                {
                    continue;
                }

                line += $"<b>{median.ToString("N0", CultureInfo.InvariantCulture)}</b> тг. ";

                var oldGradeValue = lastCacheItemOrNull?.Data.GetMedianLocalSalaryByGrade(gradeGroup);
                if (oldGradeValue is > 0)
                {
                    var diffInPercent = (median - oldGradeValue.Value) / oldGradeValue.Value * 100;

                    if (diffInPercent is > 0 or < 0)
                    {
                        hasAnyDifference = true;

                        var sign = diffInPercent > 0 ? "🔼 " : "🔻 ";
                        line += $"{sign}{diffInPercent.ToString("N0", CultureInfo.InvariantCulture)}%. ";
                    }
                }

                if (usdCurrencyOrNull != null)
                {
                    var currencyValue = (median / usdCurrencyOrNull.Value).ToString("N0", CultureInfo.InvariantCulture);
                    line += $"(~{currencyValue}{usdCurrencyOrNull.CurrencyString}) ";
                }

                line = line.Trim();
                textMessageToBeSent += line + "\n";
            }

            var calculatedBasedOnLine = $"Рассчитано на основе {subscriptionData.TotalSalaryCount} анкет(ы)";

            if (lastCacheItemOrNull is not null &&
                subscriptionData.TotalSalaryCount > lastCacheItemOrNull.Data.TotalSalaryCount)
            {
                calculatedBasedOnLine += $" (+{subscriptionData.TotalSalaryCount - lastCacheItemOrNull.Data.TotalSalaryCount})";
            }

            if (!hasAnyDifference &&
                subscription.PreventNotificationIfNoDifference)
            {
                Logger.LogInformation(
                    "No difference in salaries for subscription {SubscriptionId} ({Name}). Skipping notification.",
                    subscription.Id,
                    subscription.Name);

                continue;
            }

            var salariesChartPageLink = GetChartPageLink(
                subscription,
                subscriptionData.FilterData);

            textMessageToBeSent +=
                $"\n<em>{calculatedBasedOnLine}</em>" +
                $"\n<em>Разные графики и фильтры доступны по ссылке <a href=\"{salariesChartPageLink}\">{SalariesPageUrl}</a></em>";

            if (subscription.UseAiAnalysis)
            {
                var analysis = subscription.GetLastAiAnalysisRecordForTodayOrNull();
                if (analysis != null)
                {
                    textMessageToBeSent += $"\n\n<em>AI анализ:</em>\n{analysis.GetClearedReport()}";
                }
            }

            textMessageToBeSent += "\n\n#статистика_зарплат";

            var dataTobeSent = new TelegramBotReplyData(
                textMessageToBeSent.Trim(),
                new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithUrl(
                        text: SalariesPageUrl,
                        url: salariesChartPageLink.ToString())));

            var subscriptionRecord = new StatDataChangeSubscriptionRecord(
                subscription,
                subscriptionData.LastCacheItemOrNull,
                subscriptionData.GetStatDataCacheItemSalaryData());

            _context.Add(subscriptionRecord);
            listOfDataToBeSent.Add((subscriptionRecord, dataTobeSent));
        }

        if (listOfDataToBeSent.Count == 0)
        {
            return;
        }

        await _context.TrySaveChangesAsync(cancellationToken);

        var client = _botClientProvider.CreateClient();
        if (client is null)
        {
            Logger.LogWarning("Telegram bot is disabled.");
            return;
        }

        var failedToSend = new List<(StatDataChangeSubscriptionRecord SubscriptionRecord, Exception Ex)>();

        var hasAnySubscriptionToUpdate = false;
        foreach (var data in listOfDataToBeSent)
        {
            var result = await TrySendTelegramMessageAsync(
                data.Item,
                data.Data,
                client,
                cancellationToken);

            if (result.HasError)
            {
                failedToSend.Add((data.Item, result.RaisedException));
            }

            if (result.HasSubscription)
            {
                var subscription = result.SubscriptionToBeUpdated.Subscription;
                subscription.ChangeChatId(result.SubscriptionToBeUpdated.ChatId);
                hasAnySubscriptionToUpdate = true;
            }
        }

        if (failedToSend.Count > 0)
        {
            Logger.LogError(
                "Failed to send regular stats updates chats to {Count} users/groups. Errors: {Errors}",
                failedToSend.Count,
                failedToSend
                    .Select(x => $"Subscription {x.SubscriptionRecord?.SubscriptionId}. Error {x.Ex.Message}. Type: {x.Ex.GetType().FullName}"));
        }

        if (hasAnySubscriptionToUpdate)
        {
            await _context.TrySaveChangesAsync(cancellationToken);
        }
    }

    private ChartPageLink GetChartPageLink(
        StatDataChangeSubscription subscription,
        TelegramBotUserCommandParameters filterData)
    {
        return new ChartPageLink(_global, filterData)
            .AddQueryParam("utm_source", subscription.TelegramChatId.ToString())
            .AddQueryParam("utm_campaign", "telegram-regular-stats-update");
    }

    private async Task<StatDataChangeSubscriptionCalculateJobSendTgData> TrySendTelegramMessageAsync(
        StatDataChangeSubscriptionRecord subscriptionRecord,
        TelegramBotReplyData tgData,
        ITelegramBotClient client,
        CancellationToken cancellationToken)
    {
        try
        {
            await client.SendMessage(
                subscriptionRecord.GetChatId(),
                tgData.ReplyText,
                parseMode: tgData.ParseMode,
                replyMarkup: tgData.InlineKeyboardMarkup,
                cancellationToken: cancellationToken);

            return new StatDataChangeSubscriptionCalculateJobSendTgData();
        }
        catch (ApiRequestException apiEx)
        {
            const string chatIdChangedMessage = "Bad Request: group chat was upgraded to a supergroup chat";
            if (apiEx.Message == chatIdChangedMessage &&
                apiEx.Parameters?.MigrateToChatId != null)
            {
                await client.SendMessage(
                    apiEx.Parameters.MigrateToChatId.Value,
                    tgData.ReplyText,
                    parseMode: tgData.ParseMode,
                    replyMarkup: tgData.InlineKeyboardMarkup,
                    cancellationToken: cancellationToken);

                return new StatDataChangeSubscriptionCalculateJobSendTgData
                {
                    RaisedException = apiEx,
                    SubscriptionToBeUpdated = (subscriptionRecord.Subscription, apiEx.Parameters.MigrateToChatId.Value)
                };
            }

            return new StatDataChangeSubscriptionCalculateJobSendTgData
            {
                RaisedException = apiEx,
            };
        }
        catch (Exception e)
        {
            return new StatDataChangeSubscriptionCalculateJobSendTgData
            {
                RaisedException = e,
            };
        }
    }
}
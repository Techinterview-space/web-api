using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.StatData;
using Domain.Entities.StatData.CompanyReviews;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Infrastructure.Services.Global;
using Infrastructure.Services.Telegram.ReplyMessages;
using Infrastructure.Services.Telegram.Salaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;

namespace Web.Api.Services.CompanyReviews;

public class CompanyReviewsSubscriptionService
{
    public const string SalariesPageUrl = "techinterview.space/salaries";
    public const int CountOfDaysToSendMonthlyNotification = 24;

    private readonly DatabaseContext _context;
    private readonly IGlobal _global;
    private readonly ISalariesTelegramBotClientProvider _botClientProvider;
    private readonly ILogger<CompanyReviewsSubscriptionService> _logger;

    public CompanyReviewsSubscriptionService(
        DatabaseContext context,
        IGlobal global,
        ISalariesTelegramBotClientProvider botClientProvider,
        ILogger<CompanyReviewsSubscriptionService> logger)
    {
        _context = context;
        _global = global;
        _botClientProvider = botClientProvider;
        _logger = logger;
    }

    public Task<int> ProcessCompanyReviewsSubscriptionAsync(
        Guid subscriptionId,
        string correlationId,
        CancellationToken cancellationToken)
    {
        return ProcessAllCompanyReviewsSubscriptionsAsync(
            new List<Guid>
            {
                subscriptionId
            },
            correlationId,
            cancellationToken);
    }

    public async Task<int> ProcessAllCompanyReviewsSubscriptionsAsync(
        List<Guid> subscriptionIds,
        string correlationId,
        CancellationToken cancellationToken)
    {
        var subscriptions = await _context.CompanyReviewsSubscriptions
            .Include(x => x.TelegramMessages.OrderBy(z => z.CreatedAt))
            .Include(x => x.AiAnalysisRecords.OrderBy(z => z.CreatedAt))
            .Where(x => x.DeletedAt == null)
            .When(subscriptionIds is { Count: > 0 }, x => subscriptionIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (subscriptions.Count == 0)
        {
            _logger.LogInformation(
                "No StatDataCache records found. Exiting job. CorrelationId: {CorrelationId}",
                correlationId);
        }

        var messagesToBeSent = new List<(LastWeekCompanyReviewsSubscription Subscription, TelegramBotReplyData MessageToBeSent)>();
        var now = DateTime.UtcNow;
        foreach (var subscription in subscriptions)
        {
            var textMessageToBeSent = $"<b>Новые отзывы о компаниях</b>\n\n" +
                                      $"Дата сообщения: {now:dd.MM.yyyy HH:mm}\n" +
                                      $"AI анализ:\n";

            var aiAnalysis = subscription.GetLastAiAnalysisRecordForTodayOrNull();
            if (aiAnalysis == null)
            {
                _logger.LogWarning(
                    "No AI analysis found for subscription {SubscriptionId} ({Name}). CorrelationId: {CorrelationId}",
                    subscription.Id,
                    subscription.Name,
                    correlationId);

                continue;
            }

            textMessageToBeSent += aiAnalysis.AiReport + $"\n" +
                                   $"<em>Модель {aiAnalysis.Model}</em>\n\n";

            if (subscription.Regularity is SubscriptionRegularityType.Monthly &&
                !subscription.LastMessageWasSentDaysAgo(CountOfDaysToSendMonthlyNotification))
            {
                _logger.LogInformation(
                    "Monthly subscription {SubscriptionId} ({Name}) will be skipped due to dates",
                    subscription.Id,
                    subscription.Name);

                continue;
            }

            var skipWeeklyNotification = subscription.Regularity is SubscriptionRegularityType.Weekly &&
                                         !subscription.LastMessageWasSentDaysAgo(CountOfDaysToSendMonthlyNotification);

            if (skipWeeklyNotification)
            {
                _logger.LogInformation(
                    "No difference in salaries for subscription weekly {SubscriptionId} ({Name}). Skipping notification.",
                    subscription.Id,
                    subscription.Name);

                continue;
            }

            var salariesChartPageLink = new CompanyReviewsPageLink(_global)
                .AddQueryParam("utm_source", subscription.TelegramChatId.ToString())
                .AddQueryParam("utm_campaign", "telegram-reviews-update");

            textMessageToBeSent +=
                $"<em>Другие отзывы можно посмотреть на сайте <a href=\"{salariesChartPageLink}\">{SalariesPageUrl}</a></em>\n" +
                $"<em>Будем рады, если вы оставите <b>анонимный</b> отзыв о компании, в которой работаете или работали</em>";

            textMessageToBeSent += "\n\n#отзывы_о_компаниях";

            var messageToBeSent = new TelegramBotReplyData(
                textMessageToBeSent.Trim(),
                new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithUrl(
                        text: SalariesPageUrl,
                        url: salariesChartPageLink.ToString())));

            messagesToBeSent.Add((subscription, messageToBeSent));
        }

        var client = _botClientProvider.CreateClient();
        if (client is null)
        {
            _logger.LogWarning(
                "Telegram bot is disabled. CorrelationId: {CorrelationId}",
                correlationId);

            return 0;
        }

        var itemsToBeSaved = 0;
        foreach (var (subscription, messageToBeSent) in messagesToBeSent)
        {
            if (await TrySendTelegramMessageAsync(
                    subscription.TelegramChatId,
                    messageToBeSent,
                    client,
                    cancellationToken))
            {
                itemsToBeSaved++;
                _context.SalariesSubscriptionTelegramMessages.Add(
                    new SubscriptionTelegramMessage(
                        subscription,
                        messageToBeSent.ReplyText));
            }
            else
            {
                _logger.LogWarning(
                    "Failed to send message to chat {ChatId} for subscription {SubscriptionId} ({Name}). CorrelationId: {CorrelationId}",
                    subscription.TelegramChatId,
                    subscription.Id,
                    subscription.Name,
                    correlationId);
            }
        }

        if (itemsToBeSaved > 0)
        {
            await _context.TrySaveChangesAsync(cancellationToken);
        }

        return itemsToBeSaved;
    }

    private async Task<bool> TrySendTelegramMessageAsync(
        long telegramChatId,
        TelegramBotReplyData tgData,
        ITelegramBotClient client,
        CancellationToken cancellationToken)
    {
        try
        {
            await client.SendMessage(
                telegramChatId,
                tgData.ReplyText,
                parseMode: tgData.ParseMode,
                replyMarkup: tgData.InlineKeyboardMarkup,
                cancellationToken: cancellationToken);

            return true;
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
            }

            _logger.LogError(
                apiEx,
                "Failed to send Telegram message to chat {ChatId}",
                telegramChatId);

            return false;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Failed to send Telegram message to chat {ChatId}",
                telegramChatId);

            return false;
        }
    }
}
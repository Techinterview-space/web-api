using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Entities.StatData.CompanyReviews;

public class LastWeekCompanyReviewsSubscription : SubscriptionEntityBase
{
    public virtual List<SubscriptionTelegramMessage> TelegramMessages { get; protected set; }

    public virtual List<AiAnalysisRecord> AiAnalysisRecords { get; protected set; }

    public LastWeekCompanyReviewsSubscription(
        string name,
        long telegramChatId,
        SubscriptionRegularityType regularityType,
        bool useAiAnalysis)
        : base(name, telegramChatId, regularityType, useAiAnalysis)
    {
    }

    public void Update(
        string name,
        SubscriptionRegularityType regularityType,
        bool useAiAnalysis)
    {
        Name = name;
        Regularity = regularityType;
        UseAiAnalysis = useAiAnalysis;
    }

    protected LastWeekCompanyReviewsSubscription()
    {
    }

    // TODO remove copypaste
    public AiAnalysisRecord GetLastAiAnalysisRecordForTodayOrNull()
    {
        if (AiAnalysisRecords is null)
        {
            throw new InvalidOperationException("AI records are not loaded.");
        }

        var dayAgoEdge = DateTimeOffset.UtcNow.AddDays(-1);
        return AiAnalysisRecords
            .Where(x => x.CreatedAt >= dayAgoEdge)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();
    }

    public bool LastMessageWasSentDaysAgo(
        int daysCount)
    {
        var latestMessageSentDifference = GetDifferenceBetweenNowAndLatestSentMessage();
        return latestMessageSentDifference == null ||
               latestMessageSentDifference.Value > TimeSpan.FromDays(daysCount);
    }

    // TODO remove copypaste
    public TimeSpan? GetDifferenceBetweenNowAndLatestSentMessage()
    {
        if (TelegramMessages is null)
        {
            throw new InvalidOperationException("TelegramMessages are not loaded.");
        }

        var lastMessage = TelegramMessages
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        if (lastMessage is null)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        return now - lastMessage.CreatedAt;
    }
}
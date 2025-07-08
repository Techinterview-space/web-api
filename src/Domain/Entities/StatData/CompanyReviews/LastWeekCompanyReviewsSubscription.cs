using System.Collections.Generic;

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

    protected LastWeekCompanyReviewsSubscription()
    {
    }
}
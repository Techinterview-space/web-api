using System;

namespace Domain.Entities.StatData;

public class AiAnalysisSubscriptionRecord : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public Guid SubscriptionId { get; protected set; }

    public virtual StatDataChangeSubscription Subscription { get; protected set; }

    public string AitReportSource { get; protected set; }

    public string AiReport { get; protected set; }

    public double ProcessingTimeMs { get; protected set; }

    public AiAnalysisSubscriptionRecord(
        StatDataChangeSubscription subscription,
        string aitReportSource,
        string aiReport,
        double processingTimeMs)
    {
        Id = Guid.NewGuid();
        SubscriptionId = subscription.Id;
        Subscription = subscription;

        aitReportSource = aitReportSource?.Trim();
        aiReport = aiReport?.Trim();

        if (string.IsNullOrEmpty(aitReportSource))
        {
            throw new ArgumentNullException(nameof(aitReportSource));
        }

        if (string.IsNullOrEmpty(aiReport))
        {
            throw new ArgumentNullException(nameof(aiReport));
        }

        AitReportSource = aitReportSource;
        AiReport = aiReport;
        ProcessingTimeMs = processingTimeMs;
    }

    public string GetClearedReport()
    {
        return AiReport
            .Trim()
            .Trim('`');
    }

    public long GetChatId()
    {
        if (Subscription is null)
        {
            throw new InvalidOperationException("You must load StatDataCache before calling this method.");
        }

        return Subscription.TelegramChatId;
    }

    protected AiAnalysisSubscriptionRecord()
    {
    }
}
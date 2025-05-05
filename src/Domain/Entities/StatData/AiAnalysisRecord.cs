using System;

namespace Domain.Entities.StatData;

public class AiAnalysisRecord : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public Guid SubscriptionId { get; protected set; }

    public virtual StatDataChangeSubscription Subscription { get; protected set; }

    public string AiReportSource { get; protected set; }

    public string AiReport { get; protected set; }

    public double ProcessingTimeMs { get; protected set; }

    public AiAnalysisRecord(
        StatDataChangeSubscription subscription,
        string aiReportSource,
        string aiReport,
        double processingTimeMs)
    {
        Id = Guid.NewGuid();
        SubscriptionId = subscription.Id;
        Subscription = subscription;

        aiReportSource = aiReportSource?.Trim();
        aiReport = aiReport?.Trim();

        if (string.IsNullOrEmpty(aiReportSource))
        {
            throw new ArgumentNullException(nameof(aiReportSource));
        }

        if (string.IsNullOrEmpty(aiReport))
        {
            throw new ArgumentNullException(nameof(aiReport));
        }

        AiReportSource = aiReportSource;
        AiReport = aiReport;
        ProcessingTimeMs = processingTimeMs;
    }

    public string GetClearedReport()
    {
        return AiReport
            .Trim()
            .Trim('`')
            .Trim('\r', '\n');
    }

    public long GetChatId()
    {
        if (Subscription is null)
        {
            throw new InvalidOperationException("You must load StatDataCache before calling this method.");
        }

        return Subscription.TelegramChatId;
    }

    protected AiAnalysisRecord()
    {
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Entities.StatData;

public class StatDataChangeSubscription : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public string Name { get; protected set; }

    public long TelegramChatId { get; protected set; }

    public List<long> ProfessionIds { get; protected set; }

    /// <summary>
    /// That means that notification will not be sent if there is no difference
    /// between current and previous data.
    /// </summary>
    public bool PreventNotificationIfNoDifference { get; protected set; }

    public bool UseAiAnalysis { get; protected set; }

    public SubscriptionRegularityType Regularity { get; protected set; }

    public DateTimeOffset? DeletedAt { get; protected set; }

    public virtual List<StatDataChangeSubscriptionRecord> Records { get; protected set; }

    public virtual List<AiAnalysisRecord> AiAnalysisRecords { get; protected set; }

    public virtual List<StatDataChangeSubscriptionTgMessage> StatDataChangeSubscriptionTgMessages { get; protected set; }

    protected StatDataChangeSubscription()
    {
    }

    public StatDataChangeSubscription(
        string name,
        long telegramChatId,
        List<long> professionIds,
        bool preventNotificationIfNoDifference,
        SubscriptionRegularityType regularityType)
    {
        Id = Guid.NewGuid();
        Name = name;
        TelegramChatId = telegramChatId;
        ProfessionIds = professionIds;
        PreventNotificationIfNoDifference = preventNotificationIfNoDifference;
        DeletedAt = null;
        Regularity = regularityType;
    }

    public void Activate()
    {
        DeletedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        DeletedAt = UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangeChatId(long telegramChatId)
    {
        TelegramChatId = telegramChatId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

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

    public TimeSpan? GetDifferenceBetweenNowAndLatestSentMessage()
    {
        if (StatDataChangeSubscriptionTgMessages is null)
        {
            throw new InvalidOperationException("StatDataChangeSubscriptionTgMessages are not loaded.");
        }

        var lastMessage = StatDataChangeSubscriptionTgMessages
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
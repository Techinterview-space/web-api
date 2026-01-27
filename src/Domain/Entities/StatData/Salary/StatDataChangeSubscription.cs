using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Validation.Exceptions;

namespace Domain.Entities.StatData.Salary;

public class StatDataChangeSubscription : SubscriptionEntityBase
{
    public List<long> ProfessionIds { get; protected set; }

    /// <summary>
    /// That means that notification will not be sent if there is no difference
    /// between current and previous data.
    /// </summary>
    public bool PreventNotificationIfNoDifference { get; protected set; }

    public virtual List<StatDataChangeSubscriptionRecord> Records { get; protected set; }

    public virtual List<AiAnalysisRecord> AiAnalysisRecords { get; protected set; }

    public virtual List<SubscriptionTelegramMessage> StatDataChangeSubscriptionTgMessages { get; protected set; }

    protected StatDataChangeSubscription()
    {
    }

    public StatDataChangeSubscription(
        string name,
        long telegramChatId,
        List<long> professionIds,
        bool preventNotificationIfNoDifference,
        SubscriptionRegularityType regularityType,
        bool useAiAnalysis)
        : base(name, telegramChatId, regularityType, useAiAnalysis)
    {
        ProfessionIds = professionIds;
        PreventNotificationIfNoDifference = preventNotificationIfNoDifference;
    }

    public void Update(
        string name,
        List<long> professionIds,
        bool preventNotificationIfNoDifference,
        SubscriptionRegularityType regularityType,
        bool useAiAnalysis)
    {
        name = name?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            throw new BadRequestException("Name is required.");
        }

        Name = name;
        ProfessionIds = professionIds;
        PreventNotificationIfNoDifference = preventNotificationIfNoDifference;
        Regularity = regularityType;
        UseAiAnalysis = useAiAnalysis;

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
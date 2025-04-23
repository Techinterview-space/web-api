using System;
using System.Collections.Generic;

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

    public DateTimeOffset? DeletedAt { get; protected set; }

    public virtual List<StatDataChangeSubscriptionRecord> Records { get; protected set; }

    protected StatDataChangeSubscription()
    {
    }

    public StatDataChangeSubscription(
        string name,
        long telegramChatId,
        List<long> professionIds,
        bool preventNotificationIfNoDifference)
    {
        Id = Guid.NewGuid();
        Name = name;
        TelegramChatId = telegramChatId;
        ProfessionIds = professionIds;
        PreventNotificationIfNoDifference = preventNotificationIfNoDifference;
        DeletedAt = null;
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
}
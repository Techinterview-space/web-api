using System;
using System.Collections.Generic;

namespace Domain.Entities.StatData;

public class StatDataChangeSubscription : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public string Name { get; protected set; }

    public long TelegramChatId { get; protected set; }

    public List<long> ProfessionIds { get; protected set; }

    public DateTimeOffset? DeletedAt { get; protected set; }

    public virtual List<StatDataChangeSubscriptionRecord> StatDataChangeSubscriptionRecords { get; protected set; }

    protected StatDataChangeSubscription()
    {
    }

    public StatDataChangeSubscription(
        string name,
        long telegramChatId,
        List<long> professionIds)
    {
        Id = Guid.NewGuid();
        Name = name;
        TelegramChatId = telegramChatId;
        ProfessionIds = professionIds;
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
}
using System;
using System.Collections.Generic;

namespace Domain.Entities.StatData;

public class StatDataChangeSubscriptionRecord : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public Guid StatDataChangeSubscriptionId { get; protected set; }

    public virtual StatDataChangeSubscription StatDataChangeSubscription { get; protected set; }

    public Guid? PreviousStatDataChangeSubscriptionRecordId { get; protected set; }

    public virtual StatDataChangeSubscriptionRecord PreviousStatDataChangeSubscriptionRecord { get; protected set; }

    public virtual List<StatDataChangeSubscriptionRecord> NextStatDataChangeSubscriptionRecords { get; protected set; }

    public StatDataCacheItemSalaryData Data { get; protected set; }

    protected StatDataChangeSubscriptionRecord()
    {
    }

    public StatDataChangeSubscriptionRecord(
        StatDataChangeSubscription subscription,
        StatDataChangeSubscriptionRecord previousStatDataChangeSubscriptionRecord,
        StatDataCacheItemSalaryData data)
    {
        Id = Guid.NewGuid();
        StatDataChangeSubscriptionId = subscription.Id;
        StatDataChangeSubscription = subscription;
        PreviousStatDataChangeSubscriptionRecordId = previousStatDataChangeSubscriptionRecord?.Id;
        Data = data;
    }

    public long GetChatId()
    {
        if (StatDataChangeSubscription is null)
        {
            throw new InvalidOperationException("You must load StatDataCache before calling this method.");
        }

        return StatDataChangeSubscription.TelegramChatId;
    }
}
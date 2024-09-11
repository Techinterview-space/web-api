using System;
using System.Collections.Generic;

namespace Domain.Entities.StatData;

public class StatDataChangeSubscriptionRecord : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public Guid SubscriptionId { get; protected set; }

    public virtual StatDataChangeSubscription Subscription { get; protected set; }

    public Guid? PreviousRecordId { get; protected set; }

    public virtual StatDataChangeSubscriptionRecord PreviousRecord { get; protected set; }

    public virtual List<StatDataChangeSubscriptionRecord> NextRecords { get; protected set; }

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
        SubscriptionId = subscription.Id;
        Subscription = subscription;
        PreviousRecordId = previousStatDataChangeSubscriptionRecord?.Id;
        Data = data;
    }

    public long GetChatId()
    {
        if (Subscription is null)
        {
            throw new InvalidOperationException("You must load StatDataCache before calling this method.");
        }

        return Subscription.TelegramChatId;
    }
}
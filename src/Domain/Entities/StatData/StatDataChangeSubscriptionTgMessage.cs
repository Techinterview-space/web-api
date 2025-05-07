using System;

namespace Domain.Entities.StatData;

public class StatDataChangeSubscriptionTgMessage : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public long ChatId { get; protected set; }

    public string Message { get; protected set; }

    public Guid SubscriptionId { get; protected set; }

    public virtual StatDataChangeSubscription Subscription { get; protected set; }

    public StatDataChangeSubscriptionTgMessage(
        StatDataChangeSubscription subscription,
        string message)
    {
        SubscriptionId = subscription.Id;
        ChatId = subscription.TelegramChatId;
        Message = message;
    }

    protected StatDataChangeSubscriptionTgMessage()
    {
    }
}
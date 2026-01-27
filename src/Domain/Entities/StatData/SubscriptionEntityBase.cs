using System;

namespace Domain.Entities.StatData;

public abstract class SubscriptionEntityBase : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public string Name { get; protected set; }

    public long TelegramChatId { get; protected set; }

    public bool UseAiAnalysis { get; protected set; }

    public SubscriptionRegularityType Regularity { get; protected set; }

    public DateTimeOffset? DeletedAt { get; protected set; }

    public void Activate()
    {
        DeletedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        DeletedAt = UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangeChatId(
        long telegramChatId)
    {
        TelegramChatId = telegramChatId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    protected SubscriptionEntityBase(
        string name,
        long telegramChatId,
        SubscriptionRegularityType regularityType,
        bool useAiAnalysis)
    {
        Id = Guid.NewGuid();
        Name = name;
        TelegramChatId = telegramChatId;
        DeletedAt = null;
        Regularity = regularityType;
        UseAiAnalysis = useAiAnalysis;
        CreatedAt = UpdatedAt = DateTimeOffset.UtcNow;
    }

    protected SubscriptionEntityBase()
    {
    }
}
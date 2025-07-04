using System;
using Domain.Validation.Exceptions;

namespace Domain.Entities.Telegram;

public class SalariesBotMessage : IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public long ChatId { get; protected set; }

    public string Username { get; protected set; }

    public bool IsAdmin { get; protected set; }

    public TelegramBotUsageType UsageType { get; protected set; }

    public DateTimeOffset CreatedAt { get; protected set; }

    public SalariesBotMessage(
        long chatId,
        string username,
        TelegramBotUsageType usageType,
        bool isAdmin)
    {
        ChatId = chatId;
        Username = username?.Trim().ToLowerInvariant();
        UsageType = usageType;
        IsAdmin = isAdmin;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    protected SalariesBotMessage()
    {
    }
}
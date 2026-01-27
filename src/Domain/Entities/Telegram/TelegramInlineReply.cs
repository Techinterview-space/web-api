using System;

namespace Domain.Entities.Telegram;

public class TelegramInlineReply : IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public long? ChatId { get; protected set; }

    public string InlineQuery { get; protected set; }

    public TelegramBotType? BotType { get; protected set; }

    public DateTimeOffset CreatedAt { get; protected set; }

    public TelegramInlineReply(
        long? chatId,
        string inlineQuery,
        TelegramBotType botType)
    {
        ChatId = chatId;
        InlineQuery = inlineQuery?.Trim();
        BotType = botType;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    protected TelegramInlineReply()
    {
    }
}
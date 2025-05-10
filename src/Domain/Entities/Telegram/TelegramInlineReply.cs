using System;

namespace Domain.Entities.Telegram;

public class TelegramInlineReply : IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public string Username { get; protected set; }

    public long? ChatId { get; protected set; }

    public string ChatName { get; protected set; }

    public long UserId { get; protected set; }

    public DateTimeOffset CreatedAt { get; protected set; }

    public TelegramInlineReply(
        string username,
        long userId,
        long? chatId,
        string chatName)
    {
        Username = username;
        UserId = userId;
        ChatId = chatId;
        ChatName = chatName;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    protected TelegramInlineReply()
    {
    }
}
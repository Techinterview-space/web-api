using System;
using Domain.Validation.Exceptions;

namespace Domain.Entities.Telegram;

public class TelegramBotUsage : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public long? ChatId { get; protected set; }

    public long UsageCount { get; protected set; }

    public string Username { get; protected set; }

    public string ChannelName { get; protected set; }

    public long? ChannelId { get; protected set; }

    public string ReceivedMessageText { get; protected set; }

    public TelegramBotUsageType UsageType { get; protected set; }

    public TelegramBotUsage(
        long? chatId,
        string username,
        string channelName,
        long? channelId,
        TelegramBotUsageType usageType)
    {
        username = username?.Trim().ToLowerInvariant();
        channelName = channelName?.Trim();

        if (string.IsNullOrEmpty(username))
        {
            throw new BadRequestException("Username is not defined.");
        }

        ChatId = chatId;
        UsageCount = 0;
        Username = username;
        ChannelName = channelName;
        ChannelId = channelId;

        UsageType = usageType;
    }

    public void IncrementUsageCount(
        string receivedMessageText,
        string channelName,
        long? channelId)
    {
        UsageCount++;
        ReceivedMessageText = receivedMessageText;

        if (!string.IsNullOrEmpty(channelName))
        {
            ChannelName = channelName;
        }

        if (channelId.HasValue && channelId.Value != ChannelId)
        {
            ChannelId = channelId;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    protected TelegramBotUsage()
    {
    }
}
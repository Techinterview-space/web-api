using System;
using Domain.Validation.Exceptions;

namespace Domain.Entities.Telegram;

public class TelegramBotUsage : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public long UsageCount { get; protected set; }

    public string Username { get; protected set; }

    public string ChannelName { get; protected set; }

    public string ReceivedMessageText { get; protected set; }

    public TelegramBotUsageType UsageType { get; protected set; }

    public TelegramBotUsage(
        string username,
        string channelName,
        TelegramBotUsageType usageType)
    {
        username = username?.Trim().ToLowerInvariant();
        channelName = channelName?.Trim();

        if (string.IsNullOrEmpty(username))
        {
            throw new BadRequestException("Username is not defined.");
        }

        UsageCount = 0;
        Username = username;
        ChannelName = channelName;

        UsageType = usageType;
    }

    public void IncrementUsageCount(
        string receivedMessageText)
    {
        UsageCount++;
        ReceivedMessageText = receivedMessageText;

        UpdatedAt = DateTime.UtcNow;
    }

    protected TelegramBotUsage()
    {
    }
}
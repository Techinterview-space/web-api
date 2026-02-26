using System;
using Domain.Validation;

namespace Domain.Entities.ChannelStats;

public class MonitoredChannel : BaseModel
{
    protected MonitoredChannel()
    {
    }

    public MonitoredChannel(
        long channelExternalId,
        string channelName,
        long? discussionChatExternalId)
    {
        ChannelExternalId = channelExternalId;
        ChannelName = channelName.ThrowIfNullOrEmpty(nameof(channelName));
        DiscussionChatExternalId = discussionChatExternalId;
        IsActive = true;
    }

    public long ChannelExternalId { get; protected set; }

    public string ChannelName { get; protected set; }

    public long? DiscussionChatExternalId { get; protected set; }

    public bool IsActive { get; protected set; }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Update(
        string channelName,
        long? discussionChatExternalId)
    {
        ChannelName = channelName.ThrowIfNullOrEmpty(nameof(channelName));
        DiscussionChatExternalId = discussionChatExternalId;
    }
}

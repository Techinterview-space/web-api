namespace Web.Api.Features.ChannelStats.Channels;

public record UpdateMonitoredChannelRequest
{
    public string ChannelName { get; init; }

    public long? DiscussionChatExternalId { get; init; }

    public bool IsActive { get; init; }
}

namespace Web.Api.Features.ChannelStats.Channels;

public record CreateMonitoredChannelRequest
{
    public long ChannelExternalId { get; init; }

    public string ChannelName { get; init; }

    public long? DiscussionChatExternalId { get; init; }
}

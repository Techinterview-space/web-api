namespace Web.Api.Features.ChannelStats.Channels;

public record UpdateMonitoredChannelCommand
{
    public UpdateMonitoredChannelCommand(
        long id,
        UpdateMonitoredChannelRequest request)
    {
        Id = id;
        ChannelName = request.ChannelName;
        DiscussionChatExternalId = request.DiscussionChatExternalId;
        IsActive = request.IsActive;
    }

    public long Id { get; init; }

    public string ChannelName { get; init; }

    public long? DiscussionChatExternalId { get; init; }

    public bool IsActive { get; init; }
}

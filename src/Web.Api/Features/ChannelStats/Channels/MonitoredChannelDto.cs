using System;
using Domain.Entities.ChannelStats;

namespace Web.Api.Features.ChannelStats.Channels;

public record MonitoredChannelDto
{
    public MonitoredChannelDto()
    {
    }

    public MonitoredChannelDto(MonitoredChannel entity)
    {
        Id = entity.Id;
        ChannelExternalId = entity.ChannelExternalId;
        ChannelName = entity.ChannelName;
        DiscussionChatExternalId = entity.DiscussionChatExternalId;
        IsActive = entity.IsActive;
        CreatedAt = entity.CreatedAt;
        UpdatedAt = entity.UpdatedAt;
    }

    public long Id { get; init; }

    public long ChannelExternalId { get; init; }

    public string ChannelName { get; init; }

    public long? DiscussionChatExternalId { get; init; }

    public bool IsActive { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}

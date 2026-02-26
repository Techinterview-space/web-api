using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Microsoft.EntityFrameworkCore;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.ChannelStats.Channels;
using Xunit;

namespace Web.Api.Tests.Features.ChannelStats.Channels;

public class UpdateMonitoredChannelHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_UpdatesChannel()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake(
            channelExternalId: -1001234567890,
            channelName: "Old Name",
            discussionChatExternalId: null).PleaseAsync(context);

        context.ChangeTracker.Clear();

        var handler = new UpdateMonitoredChannelHandler(context);

        var command = new UpdateMonitoredChannelCommand(
            channel.Id,
            new UpdateMonitoredChannelRequest
            {
                ChannelName = "New Name",
                DiscussionChatExternalId = -1009876543210,
                IsActive = true,
            });

        var result = await handler.Handle(command, default);

        Assert.Equal("New Name", result.ChannelName);
        Assert.Equal(-1009876543210, result.DiscussionChatExternalId);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task Handle_DeactivateChannel_SetsIsActiveFalse()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake(
            channelExternalId: -1001234567890,
            channelName: "Active Channel").PleaseAsync(context);

        context.ChangeTracker.Clear();

        var handler = new UpdateMonitoredChannelHandler(context);

        var command = new UpdateMonitoredChannelCommand(
            channel.Id,
            new UpdateMonitoredChannelRequest
            {
                ChannelName = "Active Channel",
                IsActive = false,
            });

        var result = await handler.Handle(command, default);

        Assert.False(result.IsActive);

        var saved = await context.MonitoredChannels.FirstAsync();
        Assert.False(saved.IsActive);
    }

    [Fact]
    public async Task Handle_ReactivateChannel_SetsIsActiveTrue()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake(
            channelExternalId: -1001234567890,
            channelName: "Inactive Channel").AsInactive().PleaseAsync(context);

        context.ChangeTracker.Clear();

        var handler = new UpdateMonitoredChannelHandler(context);

        var command = new UpdateMonitoredChannelCommand(
            channel.Id,
            new UpdateMonitoredChannelRequest
            {
                ChannelName = "Inactive Channel",
                IsActive = true,
            });

        var result = await handler.Handle(command, default);

        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task Handle_NonExistentId_ThrowsNotFoundException()
    {
        await using var context = new InMemoryDatabaseContext();
        var handler = new UpdateMonitoredChannelHandler(context);

        var command = new UpdateMonitoredChannelCommand(
            99999,
            new UpdateMonitoredChannelRequest
            {
                ChannelName = "Test",
                IsActive = true,
            });

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, default));
    }

    [Fact]
    public async Task Handle_DuplicateDiscussionChatExternalId_ThrowsBadRequestException()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel1 = await new MonitoredChannelFake(
            channelExternalId: -1001234567890,
            channelName: "Channel 1",
            discussionChatExternalId: -1007777777777).PleaseAsync(context);

        var channel2 = await new MonitoredChannelFake(
            channelExternalId: -1001234567891,
            channelName: "Channel 2",
            discussionChatExternalId: -1008888888888).PleaseAsync(context);

        context.ChangeTracker.Clear();

        var handler = new UpdateMonitoredChannelHandler(context);

        var command = new UpdateMonitoredChannelCommand(
            channel2.Id,
            new UpdateMonitoredChannelRequest
            {
                ChannelName = "Channel 2",
                DiscussionChatExternalId = channel1.DiscussionChatExternalId,
                IsActive = true,
            });

        await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(command, default));
    }

    [Fact]
    public async Task Handle_EmptyChannelName_ThrowsBadRequest()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake().PleaseAsync(context);

        context.ChangeTracker.Clear();

        var handler = new UpdateMonitoredChannelHandler(context);

        var command = new UpdateMonitoredChannelCommand(
            channel.Id,
            new UpdateMonitoredChannelRequest
            {
                ChannelName = string.Empty,
                IsActive = true,
            });

        await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(command, default));
    }
}

using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Microsoft.EntityFrameworkCore;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.ChannelStats.Channels;
using Xunit;

namespace Web.Api.Tests.Features.ChannelStats.Channels;

public class CreateMonitoredChannelHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_CreatesChannel()
    {
        await using var context = new InMemoryDatabaseContext();
        var handler = new CreateMonitoredChannelHandler(context);

        var request = new CreateMonitoredChannelRequest
        {
            ChannelExternalId = -1001234567890,
            ChannelName = "My Channel",
            DiscussionChatExternalId = -1009876543210,
        };

        var result = await handler.Handle(request, default);

        Assert.Equal("My Channel", result.ChannelName);
        Assert.Equal(-1001234567890, result.ChannelExternalId);
        Assert.Equal(-1009876543210, result.DiscussionChatExternalId);
        Assert.True(result.IsActive);

        Assert.Equal(1, await context.MonitoredChannels.CountAsync());
    }

    [Fact]
    public async Task Handle_DuplicateChannelExternalId_ThrowsBadRequest()
    {
        await using var context = new InMemoryDatabaseContext();

        await new MonitoredChannelFake(
            channelExternalId: -1001234567890,
            channelName: "Existing").PleaseAsync(context);

        context.ChangeTracker.Clear();

        var handler = new CreateMonitoredChannelHandler(context);

        var request = new CreateMonitoredChannelRequest
        {
            ChannelExternalId = -1001234567890,
            ChannelName = "Duplicate",
        };

        await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(request, default));

        Assert.Equal(1, await context.MonitoredChannels.CountAsync());
    }

    [Fact]
    public async Task Handle_DuplicateDiscussionChatExternalId_ThrowsBadRequest()
    {
        await using var context = new InMemoryDatabaseContext();

        await new MonitoredChannelFake(
            channelExternalId: -1001234567890,
            channelName: "Existing",
            discussionChatExternalId: -1007777777777).PleaseAsync(context);

        context.ChangeTracker.Clear();

        var handler = new CreateMonitoredChannelHandler(context);

        var request = new CreateMonitoredChannelRequest
        {
            ChannelExternalId = -1009999999999,
            ChannelName = "Duplicate Discussion",
            DiscussionChatExternalId = -1007777777777,
        };

        await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(request, default));
    }

    [Fact]
    public async Task Handle_EmptyChannelName_ThrowsBadRequest()
    {
        await using var context = new InMemoryDatabaseContext();
        var handler = new CreateMonitoredChannelHandler(context);

        var request = new CreateMonitoredChannelRequest
        {
            ChannelExternalId = -1001234567890,
            ChannelName = string.Empty,
        };

        await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(request, default));
    }

    [Fact]
    public async Task Handle_ZeroChannelExternalId_ThrowsBadRequest()
    {
        await using var context = new InMemoryDatabaseContext();
        var handler = new CreateMonitoredChannelHandler(context);

        var request = new CreateMonitoredChannelRequest
        {
            ChannelExternalId = 0,
            ChannelName = "Test",
        };

        await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(request, default));
    }

    [Fact]
    public async Task Handle_NullDiscussionChatId_CreatesChannelWithoutDiscussion()
    {
        await using var context = new InMemoryDatabaseContext();
        var handler = new CreateMonitoredChannelHandler(context);

        var request = new CreateMonitoredChannelRequest
        {
            ChannelExternalId = -1001234567890,
            ChannelName = "No Discussion",
            DiscussionChatExternalId = null,
        };

        var result = await handler.Handle(request, default);

        Assert.Null(result.DiscussionChatExternalId);
    }
}

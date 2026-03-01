using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.ChannelStats;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.ChannelStats.GetChannelRuns;
using Xunit;

namespace Web.Api.Tests.Features.ChannelStats.GetChannelRuns;

public class GetChannelRunsHandlerTests
{
    [Fact]
    public async Task Handle_NoRuns_ReturnsEmptyList()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake().PleaseAsync(context);
        context.ChangeTracker.Clear();

        var handler = new GetChannelRunsHandler(context);

        var result = await handler.Handle(
            new GetChannelRunsQuery(channel.Id, 3),
            CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_MultipleRuns_ReturnsOrderedByCalculatedAtDesc()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake().PleaseAsync(context);

        var olderRun = CreateRun(channel.Id, new DateTimeOffset(2025, 3, 10, 12, 0, 0, TimeSpan.Zero));
        var newerRun = CreateRun(channel.Id, new DateTimeOffset(2025, 3, 15, 12, 0, 0, TimeSpan.Zero));

        await context.AddRangeAsync(olderRun, newerRun);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var handler = new GetChannelRunsHandler(context);

        var result = await handler.Handle(
            new GetChannelRunsQuery(channel.Id, 3),
            CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.True(result[0].CalculatedAtUtc > result[1].CalculatedAtUtc);
    }

    [Fact]
    public async Task Handle_TakeClamped_ReturnsAtMostRequestedCount()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake().PleaseAsync(context);

        for (var i = 0; i < 5; i++)
        {
            var run = CreateRun(channel.Id, new DateTimeOffset(2025, 3, 10 + i, 12, 0, 0, TimeSpan.Zero));
            await context.AddAsync(run);
        }

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var handler = new GetChannelRunsHandler(context);

        var result = await handler.Handle(
            new GetChannelRunsQuery(channel.Id, 2),
            CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_TakeExceedsMax_ClampedTo10()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake().PleaseAsync(context);

        for (var i = 0; i < 12; i++)
        {
            var run = CreateRun(channel.Id, new DateTimeOffset(2025, 3, 1, i, 0, 0, TimeSpan.Zero));
            await context.AddAsync(run);
        }

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var handler = new GetChannelRunsHandler(context);

        var result = await handler.Handle(
            new GetChannelRunsQuery(channel.Id, 50),
            CancellationToken.None);

        Assert.Equal(10, result.Count);
    }

    [Fact]
    public async Task Handle_DifferentChannels_ReturnsOnlyRequestedChannel()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel1 = await new MonitoredChannelFake(
            channelExternalId: -1001111111111,
            channelName: "Channel 1").PleaseAsync(context);

        var channel2 = await new MonitoredChannelFake(
            channelExternalId: -1002222222222,
            channelName: "Channel 2").PleaseAsync(context);

        await context.AddAsync(CreateRun(channel1.Id, new DateTimeOffset(2025, 3, 10, 12, 0, 0, TimeSpan.Zero)));
        await context.AddAsync(CreateRun(channel2.Id, new DateTimeOffset(2025, 3, 10, 12, 0, 0, TimeSpan.Zero)));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var handler = new GetChannelRunsHandler(context);

        var result = await handler.Handle(
            new GetChannelRunsQuery(channel1.Id, 3),
            CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(channel1.Id, result[0].MonitoredChannelId);
    }

    private static MonthlyStatsRun CreateRun(long channelId, DateTimeOffset calculatedAt)
    {
        return new MonthlyStatsRun(
            monitoredChannelId: channelId,
            month: new DateTimeOffset(calculatedAt.Year, calculatedAt.Month, 1, 0, 0, 0, TimeSpan.Zero),
            triggerSource: StatsTriggerSource.Manual,
            calculatedAtUtc: calculatedAt,
            postsCountTotal: 5,
            averagePostsPerDay: 0.5,
            mostLikedPostId: null,
            mostLikedPostRef: null,
            mostLikedPostLikes: null,
            mostCommentedPostId: null,
            mostCommentedPostRef: null,
            mostCommentedPostComments: null,
            maxPostsDayUtc: null,
            maxPostsCount: 0,
            minPostsDayUtc: null,
            minPostsCount: 0);
    }
}

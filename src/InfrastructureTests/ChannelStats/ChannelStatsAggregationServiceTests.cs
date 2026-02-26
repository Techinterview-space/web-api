using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.ChannelStats;
using Infrastructure.Services.ChannelStats;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TestUtils.Db;
using TestUtils.Fakes;
using Xunit;

namespace InfrastructureTests.ChannelStats;

public class ChannelStatsAggregationServiceTests
{
    [Fact]
    public async Task RunAsync_NoActiveChannels_ReturnsEmptyResult()
    {
        await using var context = new InMemoryDatabaseContext();

        var service = CreateService(context);

        var result = await service.RunAsync(
            StatsTriggerSource.Manual,
            DateTimeOffset.UtcNow);

        Assert.Empty(result.Runs);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task RunAsync_ChannelWithZeroPosts_ReturnsZeroAverageAndNullTopPosts()
    {
        await using var context = new InMemoryDatabaseContext();

        await new MonitoredChannelFake().PleaseAsync(context);
        context.ChangeTracker.Clear();

        var executionTime = new DateTimeOffset(2025, 3, 15, 12, 0, 0, TimeSpan.Zero);
        var service = CreateService(context);

        var result = await service.RunAsync(
            StatsTriggerSource.Manual,
            executionTime);

        Assert.Single(result.Runs);
        Assert.Empty(result.Errors);

        var run = result.Runs[0];
        Assert.Equal(0, run.PostsCountTotal);
        Assert.Equal(0, run.AveragePostsPerDay);
        Assert.Null(run.MostLikedPostId);
        Assert.Null(run.MostCommentedPostId);
    }

    [Fact]
    public async Task RunAsync_ManualTrigger_UsesExecutionTimeAsEndDate()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake().PleaseAsync(context);

        var executionTime = new DateTimeOffset(2025, 3, 10, 12, 0, 0, TimeSpan.Zero);

        await new ChannelPostFake(
            channel.Id,
            telegramMessageId: 1,
            postedAtUtc: new DateTimeOffset(2025, 3, 5, 10, 0, 0, TimeSpan.Zero)).PleaseAsync(context);

        // This post is after execution time — should NOT be included
        await new ChannelPostFake(
            channel.Id,
            telegramMessageId: 2,
            postedAtUtc: new DateTimeOffset(2025, 3, 11, 10, 0, 0, TimeSpan.Zero)).PleaseAsync(context);

        context.ChangeTracker.Clear();

        var service = CreateService(context);

        var result = await service.RunAsync(
            StatsTriggerSource.Manual,
            executionTime);

        Assert.Single(result.Runs);
        var run = result.Runs[0];
        Assert.Equal(1, run.PostsCountTotal);
    }

    [Fact]
    public async Task RunAsync_ScheduledTrigger_UsesExecutionTimeAsEndDate()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake().PleaseAsync(context);

        var executionTime = new DateTimeOffset(2025, 3, 31, 15, 0, 0, TimeSpan.Zero);

        await new ChannelPostFake(
            channel.Id,
            telegramMessageId: 1,
            postedAtUtc: new DateTimeOffset(2025, 3, 1, 8, 0, 0, TimeSpan.Zero)).PleaseAsync(context);

        await new ChannelPostFake(
            channel.Id,
            telegramMessageId: 2,
            postedAtUtc: new DateTimeOffset(2025, 3, 31, 23, 0, 0, TimeSpan.Zero)).PleaseAsync(context);

        context.ChangeTracker.Clear();

        var service = CreateService(context);

        var result = await service.RunAsync(
            StatsTriggerSource.Scheduled,
            executionTime);

        Assert.Single(result.Runs);
        var run = result.Runs[0];
        Assert.Equal(1, run.PostsCountTotal);
    }

    [Fact]
    public async Task RunAsync_MostLikedPost_TieBreaker_EarliestThenSmallestId()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake().PleaseAsync(context);

        // Post A: 10 likes, posted later
        var postA = await new ChannelPostFake(
                channel.Id,
                telegramMessageId: 1,
                postedAtUtc: new DateTimeOffset(2025, 3, 10, 12, 0, 0, TimeSpan.Zero))
            .WithLikes(10)
            .PleaseAsync(context);

        // Post B: 10 likes, posted earlier — should win tie
        var postB = await new ChannelPostFake(
                channel.Id,
                telegramMessageId: 2,
                postedAtUtc: new DateTimeOffset(2025, 3, 5, 12, 0, 0, TimeSpan.Zero))
            .WithLikes(10)
            .PleaseAsync(context);

        context.ChangeTracker.Clear();

        var executionTime = new DateTimeOffset(2025, 3, 15, 12, 0, 0, TimeSpan.Zero);
        var service = CreateService(context);

        var result = await service.RunAsync(
            StatsTriggerSource.Manual,
            executionTime);

        var run = result.Runs[0];
        Assert.Equal(postB.Id, run.MostLikedPostId);
        Assert.Equal(10, run.MostLikedPostLikes);
    }

    [Fact]
    public async Task RunAsync_MostCommentedPost_TieBreaker_EarliestThenSmallestId()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake().PleaseAsync(context);

        var postA = await new ChannelPostFake(
                channel.Id,
                telegramMessageId: 1,
                postedAtUtc: new DateTimeOffset(2025, 3, 10, 12, 0, 0, TimeSpan.Zero))
            .WithComments(5)
            .PleaseAsync(context);

        var postB = await new ChannelPostFake(
                channel.Id,
                telegramMessageId: 2,
                postedAtUtc: new DateTimeOffset(2025, 3, 5, 12, 0, 0, TimeSpan.Zero))
            .WithComments(5)
            .PleaseAsync(context);

        context.ChangeTracker.Clear();

        var executionTime = new DateTimeOffset(2025, 3, 15, 12, 0, 0, TimeSpan.Zero);
        var service = CreateService(context);

        var result = await service.RunAsync(
            StatsTriggerSource.Manual,
            executionTime);

        var run = result.Runs[0];
        Assert.Equal(postB.Id, run.MostCommentedPostId);
        Assert.Equal(5, run.MostCommentedPostComments);
    }

    [Fact]
    public async Task RunAsync_DayBuckets_IncludesZeroPostDays()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake().PleaseAsync(context);

        // All 3 posts on March 5
        for (var i = 1; i <= 3; i++)
        {
            await new ChannelPostFake(
                channel.Id,
                telegramMessageId: i,
                postedAtUtc: new DateTimeOffset(2025, 3, 5, 10 + i, 0, 0, TimeSpan.Zero)).PleaseAsync(context);
        }

        context.ChangeTracker.Clear();

        var executionTime = new DateTimeOffset(2025, 3, 10, 12, 0, 0, TimeSpan.Zero);
        var service = CreateService(context);

        var result = await service.RunAsync(
            StatsTriggerSource.Manual,
            executionTime);

        var run = result.Runs[0];

        Assert.Equal(3, run.PostsCountTotal);
        Assert.Equal(new DateTimeOffset(2025, 3, 5, 0, 0, 0, TimeSpan.Zero), run.MaxPostsDayUtc);
        Assert.Equal(3, run.MaxPostsCount);

        // Min day should be one of the zero-post days (earliest = March 1)
        Assert.Equal(0, run.MinPostsCount);
        Assert.Equal(new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero), run.MinPostsDayUtc);
    }

    [Fact]
    public async Task RunAsync_MaxPostsDay_TieBreaker_EarliestDay()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake().PleaseAsync(context);

        // 2 posts on March 3
        await new ChannelPostFake(
            channel.Id,
            telegramMessageId: 1,
            postedAtUtc: new DateTimeOffset(2025, 3, 3, 10, 0, 0, TimeSpan.Zero)).PleaseAsync(context);

        await new ChannelPostFake(
            channel.Id,
            telegramMessageId: 2,
            postedAtUtc: new DateTimeOffset(2025, 3, 3, 14, 0, 0, TimeSpan.Zero)).PleaseAsync(context);

        // 2 posts on March 7 (same count, later day)
        await new ChannelPostFake(
            channel.Id,
            telegramMessageId: 3,
            postedAtUtc: new DateTimeOffset(2025, 3, 7, 10, 0, 0, TimeSpan.Zero)).PleaseAsync(context);

        await new ChannelPostFake(
            channel.Id,
            telegramMessageId: 4,
            postedAtUtc: new DateTimeOffset(2025, 3, 7, 14, 0, 0, TimeSpan.Zero)).PleaseAsync(context);

        context.ChangeTracker.Clear();

        var executionTime = new DateTimeOffset(2025, 3, 10, 12, 0, 0, TimeSpan.Zero);
        var service = CreateService(context);

        var result = await service.RunAsync(
            StatsTriggerSource.Manual,
            executionTime);

        var run = result.Runs[0];
        Assert.Equal(new DateTimeOffset(2025, 3, 3, 0, 0, 0, TimeSpan.Zero), run.MaxPostsDayUtc);
        Assert.Equal(2, run.MaxPostsCount);
    }

    [Fact]
    public async Task RunAsync_AveragePostsPerDay_CalculatedCorrectly()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake().PleaseAsync(context);

        // 6 posts over a 10-day manual window (March 1-10)
        for (var i = 1; i <= 6; i++)
        {
            await new ChannelPostFake(
                channel.Id,
                telegramMessageId: i,
                postedAtUtc: new DateTimeOffset(2025, 3, i, 12, 0, 0, TimeSpan.Zero)).PleaseAsync(context);
        }

        context.ChangeTracker.Clear();

        var executionTime = new DateTimeOffset(2025, 3, 10, 12, 0, 0, TimeSpan.Zero);
        var service = CreateService(context);

        var result = await service.RunAsync(
            StatsTriggerSource.Manual,
            executionTime);

        var run = result.Runs[0];

        // daysInScope = (March 10 - March 1).Days + 1 = 10
        Assert.Equal(6.0 / 10, run.AveragePostsPerDay);
    }

    [Fact]
    public async Task RunAsync_InactiveChannel_IsSkipped()
    {
        await using var context = new InMemoryDatabaseContext();

        await new MonitoredChannelFake(channelName: "Active").PleaseAsync(context);
        await new MonitoredChannelFake(
            channelExternalId: -1005555555555,
            channelName: "Inactive").AsInactive().PleaseAsync(context);

        context.ChangeTracker.Clear();

        var service = CreateService(context);

        var result = await service.RunAsync(
            StatsTriggerSource.Manual,
            DateTimeOffset.UtcNow);

        Assert.Single(result.Runs);
    }

    [Fact]
    public async Task RunAsync_MultipleChannels_EachGetsOwnRun()
    {
        await using var context = new InMemoryDatabaseContext();

        var ch1 = await new MonitoredChannelFake(
            channelExternalId: -1001111111111,
            channelName: "Channel 1").PleaseAsync(context);

        var ch2 = await new MonitoredChannelFake(
            channelExternalId: -1002222222222,
            channelName: "Channel 2").PleaseAsync(context);

        await new ChannelPostFake(
            ch1.Id,
            telegramMessageId: 1,
            postedAtUtc: new DateTimeOffset(2025, 3, 5, 10, 0, 0, TimeSpan.Zero)).PleaseAsync(context);

        context.ChangeTracker.Clear();

        var executionTime = new DateTimeOffset(2025, 3, 15, 12, 0, 0, TimeSpan.Zero);
        var service = CreateService(context);

        var result = await service.RunAsync(
            StatsTriggerSource.Manual,
            executionTime);

        Assert.Equal(2, result.Runs.Count);

        var run1 = result.Runs.First(r => r.MonitoredChannelId == ch1.Id);
        var run2 = result.Runs.First(r => r.MonitoredChannelId == ch2.Id);

        Assert.Equal(1, run1.PostsCountTotal);
        Assert.Equal(0, run2.PostsCountTotal);
    }

    [Fact]
    public async Task RunAsync_PersistsRunToDatabase()
    {
        await using var context = new InMemoryDatabaseContext();

        await new MonitoredChannelFake().PleaseAsync(context);
        context.ChangeTracker.Clear();

        var service = CreateService(context);

        await service.RunAsync(
            StatsTriggerSource.Manual,
            DateTimeOffset.UtcNow);

        var savedRuns = await context.MonthlyStatsRuns.ToListAsync();
        Assert.Single(savedRuns);
        Assert.Equal(StatsTriggerSource.Manual, savedRuns[0].TriggerSource);
    }

    [Fact]
    public async Task RunAsync_February_LeapYear_CorrectDayCount()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake().PleaseAsync(context);

        await new ChannelPostFake(
            channel.Id,
            telegramMessageId: 1,
            postedAtUtc: new DateTimeOffset(2024, 2, 29, 10, 0, 0, TimeSpan.Zero)).PleaseAsync(context);

        context.ChangeTracker.Clear();

        // Scheduled run on Feb 29 (leap year)
        var executionTime = new DateTimeOffset(2024, 2, 29, 15, 0, 0, TimeSpan.Zero);
        var service = CreateService(context);

        var result = await service.RunAsync(
            StatsTriggerSource.Scheduled,
            executionTime);

        var run = result.Runs[0];

        // Full month: March 1 - Feb 1 = 29 days
        Assert.Equal(1.0 / 29, run.AveragePostsPerDay);
    }

    private static ChannelStatsAggregationService CreateService(
        InMemoryDatabaseContext context)
    {
        return new ChannelStatsAggregationService(
            context,
            new Mock<ILogger<ChannelStatsAggregationService>>().Object);
    }
}

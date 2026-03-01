using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.ChannelStats;
using Domain.Validation.Exceptions;
using Infrastructure.Services.Telegram.ChannelStats;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Requests;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.ChannelStats.SendStatsRun;
using Xunit;

namespace Web.Api.Tests.Features.ChannelStats.SendStatsRun;

public class SendStatsRunHandlerTests
{
    [Fact]
    public async Task Handle_ValidRun_SendsMessageAndReturnsSuccess()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake().PleaseAsync(context);

        var run = new MonthlyStatsRun(
            monitoredChannelId: channel.Id,
            month: new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero),
            triggerSource: StatsTriggerSource.Manual,
            calculatedAtUtc: new DateTimeOffset(2025, 3, 15, 12, 0, 0, TimeSpan.Zero),
            postsCountTotal: 10,
            averagePostsPerDay: 0.67,
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

        await context.AddAsync(run);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var mockClient = new Mock<ITelegramBotClient>();
        var mockBotProvider = new Mock<IChannelStatsBotProvider>();
        mockBotProvider
            .Setup(x => x.CreateClientAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockClient.Object);

        var handler = new SendStatsRunHandler(
            context,
            mockBotProvider.Object,
            new Mock<ILogger<SendStatsRunHandler>>().Object);

        var result = await handler.Handle(
            new SendStatsRunRequest(run.Id),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);

        mockClient.Verify(
            x => x.SendRequest(
                It.IsAny<SendMessageRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RunNotFound_ThrowsNotFoundException()
    {
        await using var context = new InMemoryDatabaseContext();

        var mockBotProvider = new Mock<IChannelStatsBotProvider>();

        var handler = new SendStatsRunHandler(
            context,
            mockBotProvider.Object,
            new Mock<ILogger<SendStatsRunHandler>>().Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                new SendStatsRunRequest(999),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_BotDisabled_ReturnsFailure()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake().PleaseAsync(context);

        var run = new MonthlyStatsRun(
            monitoredChannelId: channel.Id,
            month: new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero),
            triggerSource: StatsTriggerSource.Manual,
            calculatedAtUtc: new DateTimeOffset(2025, 3, 15, 12, 0, 0, TimeSpan.Zero),
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

        await context.AddAsync(run);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var mockBotProvider = new Mock<IChannelStatsBotProvider>();
        mockBotProvider
            .Setup(x => x.CreateClientAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ITelegramBotClient)null);

        var handler = new SendStatsRunHandler(
            context,
            mockBotProvider.Object,
            new Mock<ILogger<SendStatsRunHandler>>().Object);

        var result = await handler.Handle(
            new SendStatsRunRequest(run.Id),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Telegram bot is disabled", result.ErrorMessage);
    }

    [Fact]
    public async Task Handle_TelegramSendFails_ReturnsFailureWithGenericMessage()
    {
        await using var context = new InMemoryDatabaseContext();

        var channel = await new MonitoredChannelFake().PleaseAsync(context);

        var run = new MonthlyStatsRun(
            monitoredChannelId: channel.Id,
            month: new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero),
            triggerSource: StatsTriggerSource.Manual,
            calculatedAtUtc: new DateTimeOffset(2025, 3, 15, 12, 0, 0, TimeSpan.Zero),
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

        await context.AddAsync(run);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var mockClient = new Mock<ITelegramBotClient>();
        mockClient
            .Setup(x => x.SendRequest(
                It.IsAny<SendMessageRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Bot was blocked by the user"));

        var mockBotProvider = new Mock<IChannelStatsBotProvider>();
        mockBotProvider
            .Setup(x => x.CreateClientAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockClient.Object);

        var handler = new SendStatsRunHandler(
            context,
            mockBotProvider.Object,
            new Mock<ILogger<SendStatsRunHandler>>().Object);

        var result = await handler.Handle(
            new SendStatsRunRequest(run.Id),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Failed to send message to Telegram", result.ErrorMessage);
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.ChannelStats;
using Infrastructure.Services.ChannelStats;
using Moq;
using Web.Api.Features.ChannelStats.CalculateSingleChannelStats;
using Xunit;

namespace Web.Api.Tests.Features.ChannelStats.CalculateSingleChannelStats;

public class CalculateSingleChannelStatsHandlerTests
{
    [Fact]
    public async Task Handle_ValidChannel_ReturnsRunInResponse()
    {
        var run = new MonthlyStatsRun(
            monitoredChannelId: 1,
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

        var mockService = new Mock<IChannelStatsAggregationService>();
        mockService
            .Setup(x => x.RunForChannelAsync(
                1,
                StatsTriggerSource.Manual,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(run);

        var handler = new CalculateSingleChannelStatsHandler(mockService.Object);

        var result = await handler.Handle(
            new CalculateSingleChannelStatsRequest(1),
            CancellationToken.None);

        Assert.Single(result.Runs);
        Assert.Empty(result.Errors);
        Assert.Equal(10, result.Runs[0].PostsCountTotal);
    }

    [Fact]
    public async Task Handle_ServiceThrows_ExceptionPropagates()
    {
        var mockService = new Mock<IChannelStatsAggregationService>();
        mockService
            .Setup(x => x.RunForChannelAsync(
                999,
                StatsTriggerSource.Manual,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Domain.Validation.Exceptions.NotFoundException("Not found"));

        var handler = new CalculateSingleChannelStatsHandler(mockService.Object);

        await Assert.ThrowsAsync<Domain.Validation.Exceptions.NotFoundException>(
            () => handler.Handle(
                new CalculateSingleChannelStatsRequest(999),
                CancellationToken.None));
    }
}

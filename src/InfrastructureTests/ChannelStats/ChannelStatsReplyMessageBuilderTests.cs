using System;
using Domain.Entities.ChannelStats;
using Infrastructure.Services.Telegram.ReplyMessages;
using Telegram.Bot.Types.Enums;
using Xunit;

namespace InfrastructureTests.ChannelStats;

public class ChannelStatsReplyMessageBuilderTests
{
    [Fact]
    public void Build_FullStats_ContainsAllSections()
    {
        var run = CreateRun(
            postsCountTotal: 42,
            averagePostsPerDay: 1.4,
            mostLikedPostId: 100,
            mostLikedPostRef: "https://t.me/channel/100",
            mostLikedPostLikes: 25,
            mostCommentedPostId: 200,
            mostCommentedPostRef: "https://t.me/channel/200",
            mostCommentedPostComments: 15,
            maxPostsDayUtc: new DateTimeOffset(2025, 3, 15, 0, 0, 0, TimeSpan.Zero),
            maxPostsCount: 5,
            minPostsDayUtc: new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero),
            minPostsCount: 0);

        var result = ChannelStatsReplyMessageBuilder.Build(run, "Test Channel");

        Assert.Equal(ParseMode.Html, result.ParseMode);
        Assert.Contains("Test Channel", result.ReplyText);
        Assert.Contains("42", result.ReplyText);
        Assert.Contains("1.4", result.ReplyText);
        Assert.Contains("15.03.2025", result.ReplyText);
        Assert.Contains("5 постов", result.ReplyText);
        Assert.Contains("01.03.2025", result.ReplyText);
        Assert.Contains("0 постов", result.ReplyText);
        Assert.Contains("25", result.ReplyText);
        Assert.Contains("https://t.me/channel/100", result.ReplyText);
        Assert.Contains("15", result.ReplyText);
        Assert.Contains("https://t.me/channel/200", result.ReplyText);
    }

    [Fact]
    public void Build_ZeroPosts_OmitsTopPostsAndDaySections()
    {
        var run = CreateRun(
            postsCountTotal: 0,
            averagePostsPerDay: 0);

        var result = ChannelStatsReplyMessageBuilder.Build(run, "Empty Channel");

        Assert.Contains("Empty Channel", result.ReplyText);
        Assert.Contains("0", result.ReplyText);
        Assert.DoesNotContain("Самый активный день", result.ReplyText);
        Assert.DoesNotContain("Самый популярный пост", result.ReplyText);
        Assert.DoesNotContain("Самый обсуждаемый пост", result.ReplyText);
    }

    [Fact]
    public void Build_MostLikedWithoutRef_OmitsLink()
    {
        var run = CreateRun(
            postsCountTotal: 5,
            averagePostsPerDay: 0.5,
            mostLikedPostId: 100,
            mostLikedPostRef: null,
            mostLikedPostLikes: 10);

        var result = ChannelStatsReplyMessageBuilder.Build(run, "Channel");

        Assert.Contains("10", result.ReplyText);
        Assert.DoesNotContain("ссылка", result.ReplyText);
    }

    [Fact]
    public void Build_ChannelNameWithHtmlChars_EscapesCorrectly()
    {
        var run = CreateRun(
            postsCountTotal: 1,
            averagePostsPerDay: 1.0);

        var result = ChannelStatsReplyMessageBuilder.Build(run, "News <Tech> & More");

        Assert.Contains("News &lt;Tech&gt; &amp; More", result.ReplyText);
        Assert.DoesNotContain("News <Tech>", result.ReplyText);
    }

    private static MonthlyStatsRun CreateRun(
        int postsCountTotal,
        double averagePostsPerDay,
        long? mostLikedPostId = null,
        string mostLikedPostRef = null,
        int? mostLikedPostLikes = null,
        long? mostCommentedPostId = null,
        string mostCommentedPostRef = null,
        int? mostCommentedPostComments = null,
        DateTimeOffset? maxPostsDayUtc = null,
        int maxPostsCount = 0,
        DateTimeOffset? minPostsDayUtc = null,
        int minPostsCount = 0)
    {
        return new MonthlyStatsRun(
            monitoredChannelId: 1,
            month: new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero),
            triggerSource: StatsTriggerSource.Scheduled,
            calculatedAtUtc: new DateTimeOffset(2025, 3, 31, 15, 0, 0, TimeSpan.Zero),
            postsCountTotal: postsCountTotal,
            averagePostsPerDay: averagePostsPerDay,
            mostLikedPostId: mostLikedPostId,
            mostLikedPostRef: mostLikedPostRef,
            mostLikedPostLikes: mostLikedPostLikes,
            mostCommentedPostId: mostCommentedPostId,
            mostCommentedPostRef: mostCommentedPostRef,
            mostCommentedPostComments: mostCommentedPostComments,
            maxPostsDayUtc: maxPostsDayUtc,
            maxPostsCount: maxPostsCount,
            minPostsDayUtc: minPostsDayUtc,
            minPostsCount: minPostsCount);
    }
}

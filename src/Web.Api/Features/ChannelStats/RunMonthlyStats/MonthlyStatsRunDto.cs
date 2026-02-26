using System;
using Domain.Entities.ChannelStats;

namespace Web.Api.Features.ChannelStats.RunMonthlyStats;

public record MonthlyStatsRunDto
{
    public MonthlyStatsRunDto()
    {
    }

    public MonthlyStatsRunDto(MonthlyStatsRun entity)
    {
        Id = entity.Id;
        MonitoredChannelId = entity.MonitoredChannelId;
        Month = entity.Month;
        TriggerSource = entity.TriggerSource;
        TriggerSourceAsString = entity.TriggerSource.ToString();
        CalculatedAtUtc = entity.CalculatedAtUtc;
        PostsCountTotal = entity.PostsCountTotal;
        AveragePostsPerDay = entity.AveragePostsPerDay;
        MostLikedPostId = entity.MostLikedPostId;
        MostLikedPostRef = entity.MostLikedPostRef;
        MostLikedPostLikes = entity.MostLikedPostLikes;
        MostCommentedPostId = entity.MostCommentedPostId;
        MostCommentedPostRef = entity.MostCommentedPostRef;
        MostCommentedPostComments = entity.MostCommentedPostComments;
        MaxPostsDayUtc = entity.MaxPostsDayUtc;
        MaxPostsCount = entity.MaxPostsCount;
        MinPostsDayUtc = entity.MinPostsDayUtc;
        MinPostsCount = entity.MinPostsCount;
    }

    public long Id { get; init; }

    public long MonitoredChannelId { get; init; }

    public DateTimeOffset Month { get; init; }

    public StatsTriggerSource TriggerSource { get; init; }

    public string TriggerSourceAsString { get; init; }

    public DateTimeOffset CalculatedAtUtc { get; init; }

    public int PostsCountTotal { get; init; }

    public double AveragePostsPerDay { get; init; }

    public long? MostLikedPostId { get; init; }

    public string MostLikedPostRef { get; init; }

    public int? MostLikedPostLikes { get; init; }

    public long? MostCommentedPostId { get; init; }

    public string MostCommentedPostRef { get; init; }

    public int? MostCommentedPostComments { get; init; }

    public DateTimeOffset? MaxPostsDayUtc { get; init; }

    public int MaxPostsCount { get; init; }

    public DateTimeOffset? MinPostsDayUtc { get; init; }

    public int MinPostsCount { get; init; }
}

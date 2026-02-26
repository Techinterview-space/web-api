using System;

namespace Domain.Entities.ChannelStats;

public class MonthlyStatsRun : BaseModel
{
    protected MonthlyStatsRun()
    {
    }

    public MonthlyStatsRun(
        long monitoredChannelId,
        DateTimeOffset month,
        StatsTriggerSource triggerSource,
        DateTimeOffset calculatedAtUtc,
        int postsCountTotal,
        double averagePostsPerDay,
        long? mostLikedPostId,
        string mostLikedPostRef,
        int? mostLikedPostLikes,
        long? mostCommentedPostId,
        string mostCommentedPostRef,
        int? mostCommentedPostComments,
        DateTimeOffset? maxPostsDayUtc,
        int maxPostsCount,
        DateTimeOffset? minPostsDayUtc,
        int minPostsCount)
    {
        MonitoredChannelId = monitoredChannelId;
        Month = month;
        TriggerSource = triggerSource;
        CalculatedAtUtc = calculatedAtUtc;
        PostsCountTotal = postsCountTotal;
        AveragePostsPerDay = averagePostsPerDay;
        MostLikedPostId = mostLikedPostId;
        MostLikedPostRef = mostLikedPostRef;
        MostLikedPostLikes = mostLikedPostLikes;
        MostCommentedPostId = mostCommentedPostId;
        MostCommentedPostRef = mostCommentedPostRef;
        MostCommentedPostComments = mostCommentedPostComments;
        MaxPostsDayUtc = maxPostsDayUtc;
        MaxPostsCount = maxPostsCount;
        MinPostsDayUtc = minPostsDayUtc;
        MinPostsCount = minPostsCount;
    }

    public long MonitoredChannelId { get; protected set; }

    public virtual MonitoredChannel MonitoredChannel { get; protected set; }

    public DateTimeOffset Month { get; protected set; }

    public StatsTriggerSource TriggerSource { get; protected set; }

    public DateTimeOffset CalculatedAtUtc { get; protected set; }

    public int PostsCountTotal { get; protected set; }

    public double AveragePostsPerDay { get; protected set; }

    public long? MostLikedPostId { get; protected set; }

    public string MostLikedPostRef { get; protected set; }

    public int? MostLikedPostLikes { get; protected set; }

    public long? MostCommentedPostId { get; protected set; }

    public string MostCommentedPostRef { get; protected set; }

    public int? MostCommentedPostComments { get; protected set; }

    public DateTimeOffset? MaxPostsDayUtc { get; protected set; }

    public int MaxPostsCount { get; protected set; }

    public DateTimeOffset? MinPostsDayUtc { get; protected set; }

    public int MinPostsCount { get; protected set; }
}

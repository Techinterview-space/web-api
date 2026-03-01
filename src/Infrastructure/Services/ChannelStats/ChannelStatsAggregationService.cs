using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.ChannelStats;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.ChannelStats;

public class ChannelStatsAggregationService : IChannelStatsAggregationService
{
    private readonly DatabaseContext _context;
    private readonly ILogger<ChannelStatsAggregationService> _logger;

    public ChannelStatsAggregationService(
        DatabaseContext context,
        ILogger<ChannelStatsAggregationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ChannelStatsAggregationResult> RunAsync(
        StatsTriggerSource triggerSource,
        DateTimeOffset executionTimeUtc,
        CancellationToken cancellationToken = default)
    {
        executionTimeUtc = executionTimeUtc.ToUniversalTime();

        var monthStartUtc = new DateTimeOffset(
            executionTimeUtc.Year,
            executionTimeUtc.Month,
            1,
            0,
            0,
            0,
            TimeSpan.Zero);

        var endUtc = executionTimeUtc;

        var channels = await _context.MonitoredChannels
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);

        if (channels.Count == 0)
        {
            _logger.LogInformation("No active monitored channels found, skipping aggregation");
            return new ChannelStatsAggregationResult(
                new List<MonthlyStatsRun>(),
                new List<ChannelStatsAggregationError>(),
                new Dictionary<long, MonitoredChannel>());
        }

        var runs = new List<MonthlyStatsRun>();
        var errors = new List<ChannelStatsAggregationError>();

        foreach (var channel in channels)
        {
            try
            {
                var run = await AggregateChannelAsync(
                    channel, triggerSource, executionTimeUtc, monthStartUtc, endUtc, cancellationToken);
                runs.Add(run);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to aggregate stats for channel {ChannelName} (Id: {ChannelId})",
                    channel.ChannelName,
                    channel.Id);

                errors.Add(new ChannelStatsAggregationError(
                    channel.Id, channel.ChannelName, ex.Message));
            }
        }

        if (runs.Count > 0)
        {
            await _context.TrySaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Aggregation complete. Channels processed: {Processed}, failed: {Failed}, trigger: {Trigger}",
            runs.Count,
            errors.Count,
            triggerSource);

        var channelsDictionary = channels.ToDictionary(x => x.Id);
        return new ChannelStatsAggregationResult(runs, errors, channelsDictionary);
    }

    private async Task<MonthlyStatsRun> AggregateChannelAsync(
        MonitoredChannel channel,
        StatsTriggerSource triggerSource,
        DateTimeOffset executionTimeUtc,
        DateTimeOffset monthStartUtc,
        DateTimeOffset endUtc,
        CancellationToken cancellationToken)
    {
        var posts = await _context.ChannelPosts
            .AsNoTracking()
            .Where(x => x.MonitoredChannelId == channel.Id
                        && x.PostedAtUtc >= monthStartUtc
                        && x.PostedAtUtc <= endUtc)
            .ToListAsync(cancellationToken);

        var totalPosts = posts.Count;
        var daysInScope = (endUtc.Date - monthStartUtc.Date).Days + 1;
        if (daysInScope < 1)
        {
            daysInScope = 1;
        }

        var averagePostsPerDay = (double)totalPosts / daysInScope;

        ChannelPost mostLikedPost = null;
        ChannelPost mostCommentedPost = null;
        DateTimeOffset? maxPostsDayUtc = null;
        int maxPostsCount = 0;
        DateTimeOffset? minPostsDayUtc = null;
        int minPostsCount = 0;

        if (totalPosts > 0)
        {
            mostLikedPost = posts
                .OrderByDescending(x => x.LikeCount)
                .ThenBy(x => x.PostedAtUtc)
                .ThenBy(x => x.Id)
                .First();

            mostCommentedPost = posts
                .OrderByDescending(x => x.CommentCount)
                .ThenBy(x => x.PostedAtUtc)
                .ThenBy(x => x.Id)
                .First();
        }

        var dayBuckets = BuildDayBuckets(posts, monthStartUtc, endUtc);

        if (dayBuckets.Count > 0)
        {
            var maxBucket = dayBuckets
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key)
                .First();

            var minBucket = dayBuckets
                .OrderBy(x => x.Value)
                .ThenBy(x => x.Key)
                .First();

            maxPostsDayUtc = maxBucket.Key;
            maxPostsCount = maxBucket.Value;
            minPostsDayUtc = minBucket.Key;
            minPostsCount = minBucket.Value;
        }

        var run = new MonthlyStatsRun(
            monitoredChannelId: channel.Id,
            month: monthStartUtc,
            triggerSource: triggerSource,
            calculatedAtUtc: executionTimeUtc,
            postsCountTotal: totalPosts,
            averagePostsPerDay: averagePostsPerDay,
            mostLikedPostId: mostLikedPost?.Id,
            mostLikedPostRef: mostLikedPost?.PostReference,
            mostLikedPostLikes: mostLikedPost?.LikeCount,
            mostCommentedPostId: mostCommentedPost?.Id,
            mostCommentedPostRef: mostCommentedPost?.PostReference,
            mostCommentedPostComments: mostCommentedPost?.CommentCount,
            maxPostsDayUtc: maxPostsDayUtc,
            maxPostsCount: maxPostsCount,
            minPostsDayUtc: minPostsDayUtc,
            minPostsCount: minPostsCount);

        await _context.AddAsync(run, cancellationToken);

        _logger.LogInformation(
            "Aggregated stats for channel {ChannelName}: {TotalPosts} posts, avg {Avg:F2}/day",
            channel.ChannelName,
            totalPosts,
            averagePostsPerDay);

        return run;
    }

    private static Dictionary<DateTimeOffset, int> BuildDayBuckets(
        List<ChannelPost> posts,
        DateTimeOffset monthStartUtc,
        DateTimeOffset endUtc)
    {
        var buckets = new Dictionary<DateTimeOffset, int>();

        var currentDay = monthStartUtc;
        while (currentDay.Date <= endUtc.Date)
        {
            buckets[currentDay] = 0;
            currentDay = currentDay.AddDays(1);
        }

        foreach (var post in posts)
        {
            var dayKey = new DateTimeOffset(
                post.PostedAtUtc.Year,
                post.PostedAtUtc.Month,
                post.PostedAtUtc.Day,
                0,
                0,
                0,
                TimeSpan.Zero);

            if (buckets.ContainsKey(dayKey))
            {
                buckets[dayKey]++;
            }
            else
            {
                buckets[dayKey] = 1;
            }
        }

        return buckets;
    }
}

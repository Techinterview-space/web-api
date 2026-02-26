using System;
using System.Threading.Tasks;
using Domain.Entities.ChannelStats;
using Microsoft.EntityFrameworkCore;
using TestUtils.Db;

namespace TestUtils.Fakes;

public class ChannelPostFake : ChannelPost, IPlease<ChannelPost>
{
    public ChannelPostFake(
        long monitoredChannelId,
        long telegramMessageId = 1,
        DateTimeOffset? postedAtUtc = null,
        string postReference = null,
        string textPreview = "Test post text")
        : base(
            monitoredChannelId,
            telegramMessageId,
            postedAtUtc ?? DateTimeOffset.UtcNow,
            postReference ?? $"https://t.me/test/{telegramMessageId}",
            textPreview)
    {
    }

    public ChannelPostFake WithLikes(int count)
    {
        UpdateLikeCount(count);
        return this;
    }

    public ChannelPostFake WithComments(int count)
    {
        UpdateCommentCount(count);
        return this;
    }

    public ChannelPost Please() => this;

    public IPlease<ChannelPost> AsPlease() => this;

    public async Task<ChannelPost> PleaseAsync(DbContext context)
    {
        var entry = await context.AddAsync(Please());
        await context.SaveChangesAsync();
        return entry.Entity;
    }
}

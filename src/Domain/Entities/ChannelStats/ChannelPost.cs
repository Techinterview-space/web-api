using System;
using Domain.Validation;

namespace Domain.Entities.ChannelStats;

public class ChannelPost : BaseModel
{
    protected ChannelPost()
    {
    }

    public ChannelPost(
        long monitoredChannelId,
        long telegramMessageId,
        DateTimeOffset postedAtUtc,
        string postReference,
        string textPreview)
    {
        MonitoredChannelId = monitoredChannelId;
        TelegramMessageId = telegramMessageId;
        PostedAtUtc = postedAtUtc;
        PostReference = postReference;
        TextPreview = textPreview;
        LikeCount = 0;
        CommentCount = 0;
    }

    public long MonitoredChannelId { get; protected set; }

    public virtual MonitoredChannel MonitoredChannel { get; protected set; }

    public long TelegramMessageId { get; protected set; }

    public DateTimeOffset PostedAtUtc { get; protected set; }

    public string PostReference { get; protected set; }

    public string TextPreview { get; protected set; }

    public int LikeCount { get; protected set; }

    public int CommentCount { get; protected set; }

    public void UpdateLikeCount(int likeCount)
    {
        LikeCount = likeCount;
    }

    public void IncrementCommentCount()
    {
        CommentCount++;
    }

    public void UpdateCommentCount(int commentCount)
    {
        CommentCount = commentCount;
    }

    public void UpdateTextPreview(string textPreview)
    {
        TextPreview = textPreview;
    }
}

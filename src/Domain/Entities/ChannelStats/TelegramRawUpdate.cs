using System;
using Domain.Validation;

namespace Domain.Entities.ChannelStats;

public class TelegramRawUpdate : BaseModel
{
    protected TelegramRawUpdate()
    {
    }

    public TelegramRawUpdate(
        long updateId,
        string payloadJson)
    {
        UpdateId = updateId;
        PayloadJson = payloadJson.ThrowIfNullOrEmpty(nameof(payloadJson));
        Status = TelegramUpdateStatus.Pending;
    }

    public long UpdateId { get; protected set; }

    public DateTimeOffset ReceivedAtUtc { get; protected set; } = DateTimeOffset.UtcNow;

    public string PayloadJson { get; protected set; }

    public DateTimeOffset? ProcessedAtUtc { get; protected set; }

    public TelegramUpdateStatus Status { get; protected set; }

    public string Error { get; protected set; }

    public void MarkProcessed()
    {
        Status = TelegramUpdateStatus.Processed;
        ProcessedAtUtc = DateTimeOffset.UtcNow;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Status = TelegramUpdateStatus.Failed;
        ProcessedAtUtc = DateTimeOffset.UtcNow;
        Error = error;
    }
}

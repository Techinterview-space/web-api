using System;
using System.Collections.Generic;
using Domain.ValueObjects;

namespace Domain.Entities.StatData;

public class StatDataCache : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public string Name { get; protected set; }

    public long TelegramChatId { get; protected set; }

    public List<long> ProfessionIds { get; protected set; }

    public DateTimeOffset? DeletedAt { get; protected set; }

    public virtual List<StatDataCacheItem> StatDataCacheItems { get; protected set; }

    protected StatDataCache()
    {
    }

    public StatDataCache(
        string name,
        long telegramChatId,
        List<long> professionIds)
    {
        Id = Guid.NewGuid();
        Name = name;
        TelegramChatId = telegramChatId;
        ProfessionIds = professionIds;
        DeletedAt = null;
    }

    public void Activate()
    {
        DeletedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        DeletedAt = UpdatedAt = DateTimeOffset.UtcNow;
    }
}
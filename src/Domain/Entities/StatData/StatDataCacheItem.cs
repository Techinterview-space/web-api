using System;

namespace Domain.Entities.StatData;

public class StatDataCacheItem : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public Guid StatDataCacheId { get; protected set; }

    public virtual StatDataCache StatDataCache { get; protected set; }

    public Guid? PreviousStatDataCacheItemId { get; protected set; }

    public virtual StatDataCacheItem PreviousStatDataCacheItem { get; protected set; }

    public virtual StatDataCacheItem NextStatDataCacheItem { get; protected set; }

    public StatDataCacheItemSalaryData Data { get; protected set; }

    protected StatDataCacheItem()
    {
    }

    public StatDataCacheItem(
        StatDataCache cache,
        StatDataCacheItem previousStatDataCacheItem,
        StatDataCacheItemSalaryData data)
    {
        Id = Guid.NewGuid();
        StatDataCacheId = cache.Id;
        StatDataCache = cache;
        PreviousStatDataCacheItemId = previousStatDataCacheItem?.Id;
        Data = data;
    }

    public long GetChatId()
    {
        if (StatDataCache is null)
        {
            throw new InvalidOperationException("You must load StatDataCache before calling this method.");
        }

        return StatDataCache.TelegramChatId;
    }
}
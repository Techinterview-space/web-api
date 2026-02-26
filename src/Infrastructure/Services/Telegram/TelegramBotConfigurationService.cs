using Domain.Entities.Telegram;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Telegram;

public class TelegramBotConfigurationService : ITelegramBotConfigurationService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private readonly DatabaseContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TelegramBotConfigurationService> _logger;

    public TelegramBotConfigurationService(
        DatabaseContext context,
        IMemoryCache cache,
        ILogger<TelegramBotConfigurationService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<TelegramBotConfigurationCacheItem> GetByBotTypeAsync(
        TelegramBotType botType,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"TelegramBotConfig_{botType}";

        if (_cache.TryGetValue(cacheKey, out TelegramBotConfigurationCacheItem cached))
        {
            return cached;
        }

        var config = await _context.TelegramBotConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BotType == botType, cancellationToken);

        if (config == null)
        {
            _logger.LogWarning(
                "Telegram bot DB config not found for {BotType}",
                botType);

            return null;
        }

        var cacheItem = new TelegramBotConfigurationCacheItem(config);
        _cache.Set(cacheKey, cacheItem, CacheDuration);
        return cacheItem;
    }

    public void InvalidateCache(TelegramBotType botType)
    {
        var cacheKey = $"TelegramBotConfig_{botType}";
        _cache.Remove(cacheKey);
    }
}

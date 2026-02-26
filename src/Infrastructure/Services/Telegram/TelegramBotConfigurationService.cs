using Domain.Entities.Telegram;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Telegram;

public class TelegramBotConfigurationService : ITelegramBotConfigurationService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private readonly DatabaseContext _context;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramBotConfigurationService> _logger;

    public TelegramBotConfigurationService(
        DatabaseContext context,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<TelegramBotConfigurationService> logger)
    {
        _context = context;
        _cache = cache;
        _configuration = configuration;
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

        if (config != null)
        {
            var cacheItem = new TelegramBotConfigurationCacheItem(config);
            _cache.Set(cacheKey, cacheItem, CacheDuration);
            return cacheItem;
        }

        _logger.LogInformation(
            "Telegram bot DB config not found for {BotType}, falling back to legacy config",
            botType);

        var legacyItem = BuildFromLegacyConfig(botType);
        if (legacyItem != null)
        {
            _cache.Set(cacheKey, legacyItem, CacheDuration);
        }

        return legacyItem;
    }

    public void InvalidateCache(TelegramBotType botType)
    {
        var cacheKey = $"TelegramBotConfig_{botType}";
        _cache.Remove(cacheKey);
    }

    private TelegramBotConfigurationCacheItem BuildFromLegacyConfig(TelegramBotType botType)
    {
        var (enableKey, configTokenKey, envTokenKey) = GetLegacyConfigKeys(botType);
        if (enableKey == null)
        {
            return null;
        }

        var enabled = _configuration[enableKey]?.ToLowerInvariant();
        var parsedEnabled = bool.TryParse(enabled, out var isEnabled);

        var token = Environment.GetEnvironmentVariable(envTokenKey);
        if (string.IsNullOrEmpty(token))
        {
            token = _configuration[configTokenKey];
        }

        if (!parsedEnabled || !isEnabled || string.IsNullOrEmpty(token))
        {
            _logger.LogWarning(
                "Telegram bot {BotType} is disabled via legacy config. Value {Value}. Parsed: {Parsed}",
                botType,
                enabled,
                parsedEnabled);

            return null;
        }

        return new TelegramBotConfigurationCacheItem
        {
            BotType = botType,
            DisplayName = botType.ToString(),
            Token = token,
            IsEnabled = true,
        };
    }

    private static (string EnableKey, string ConfigTokenKey, string EnvTokenKey) GetLegacyConfigKeys(
        TelegramBotType botType)
    {
        return botType switch
        {
            TelegramBotType.Salaries => (
                "Telegram:SalariesBotEnable",
                "Telegram:SalariesBotToken",
                "Telegram__SalariesBotToken"),
            TelegramBotType.GithubProfile => (
                "Telegram:GithubProfileBotEnable",
                "Telegram:GithubProfileBotToken",
                "Telegram__GithubProfileBotToken"),
            _ => (null, null, null),
        };
    }
}

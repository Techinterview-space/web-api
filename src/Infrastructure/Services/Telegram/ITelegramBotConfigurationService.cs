using Domain.Entities.Telegram;

namespace Infrastructure.Services.Telegram;

public interface ITelegramBotConfigurationService
{
    Task<TelegramBotConfigurationCacheItem> GetByBotTypeAsync(
        TelegramBotType botType,
        CancellationToken cancellationToken = default);

    void InvalidateCache(TelegramBotType botType);
}

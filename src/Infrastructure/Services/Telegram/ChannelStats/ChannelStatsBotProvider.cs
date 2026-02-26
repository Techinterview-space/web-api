using Domain.Entities.Telegram;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Infrastructure.Services.Telegram.ChannelStats;

public class ChannelStatsBotProvider : IChannelStatsBotProvider
{
    private readonly ITelegramBotConfigurationService _botConfigurationService;
    private readonly ILogger<ChannelStatsBotProvider> _logger;

    public ChannelStatsBotProvider(
        ITelegramBotConfigurationService botConfigurationService,
        ILogger<ChannelStatsBotProvider> logger)
    {
        _botConfigurationService = botConfigurationService;
        _logger = logger;
    }

    public async Task<ITelegramBotClient> CreateClientAsync(
        CancellationToken cancellationToken = default)
    {
        var config = await _botConfigurationService.GetByBotTypeAsync(
            TelegramBotType.ChannelStats, cancellationToken);

        if (config == null)
        {
            _logger.LogWarning("ChannelStats Telegram bot configuration not found");
            return null;
        }

        if (!config.IsAvailableForProcessing())
        {
            _logger.LogWarning(
                "ChannelStats Telegram bot is disabled or missing token. IsEnabled: {IsEnabled}",
                config.IsEnabled);
            return null;
        }

        return new TelegramBotClient(config.Token);
    }
}

using System.Threading.Tasks;
using Domain.Entities.Telegram;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Infrastructure.Services.Telegram.Salaries;

public class SalariesTelegramBotClientProvider : ISalariesTelegramBotClientProvider
{
    private readonly ITelegramBotConfigurationService _botConfigurationService;
    private readonly ILogger<SalariesTelegramBotClientProvider> _logger;

    public SalariesTelegramBotClientProvider(
        ITelegramBotConfigurationService botConfigurationService,
        ILogger<SalariesTelegramBotClientProvider> logger)
    {
        _botConfigurationService = botConfigurationService;
        _logger = logger;
    }

    public async Task<ITelegramBotClient> CreateClientAsync(
        CancellationToken cancellationToken = default)
    {
        var config = await _botConfigurationService.GetByBotTypeAsync(TelegramBotType.Salaries, cancellationToken);
        return CreateClientFromConfig(config);
    }

    private ITelegramBotClient CreateClientFromConfig(TelegramBotConfigurationCacheItem config)
    {
        if (config == null)
        {
            _logger.LogWarning("Salaries Telegram bot configuration not found");
            return null;
        }

        if (!config.IsAvailableForProcessing())
        {
            _logger.LogWarning(
                "Salaries Telegram bot is disabled or missing token. IsEnabled: {IsEnabled}",
                config.IsEnabled);
            return null;
        }

        return new TelegramBotClient(config.Token);
    }
}

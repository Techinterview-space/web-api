using System.Threading.Tasks;
using Domain.Entities.Telegram;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Infrastructure.Services.Telegram.GithubProfile;

public class GithubProfileBotProvider : IGithubProfileBotProvider
{
    private readonly ITelegramBotConfigurationService _botConfigurationService;
    private readonly ILogger<GithubProfileBotProvider> _logger;

    public GithubProfileBotProvider(
        ITelegramBotConfigurationService botConfigurationService,
        ILogger<GithubProfileBotProvider> logger)
    {
        _botConfigurationService = botConfigurationService;
        _logger = logger;
    }

    public async Task<ITelegramBotClient> CreateClientAsync(
        CancellationToken cancellationToken = default)
    {
        var config = await _botConfigurationService.GetByBotTypeAsync(TelegramBotType.GithubProfile, cancellationToken);
        return CreateClientFromConfig(config);
    }

    private ITelegramBotClient CreateClientFromConfig(
        TelegramBotConfigurationCacheItem config)
    {
        if (config == null)
        {
            _logger.LogWarning("Github profile Telegram bot configuration not found");
            return null;
        }

        if (!config.IsAvailableForProcessing())
        {
            _logger.LogWarning(
                "Github profile Telegram bot is disabled or missing token. IsEnabled: {IsEnabled}",
                config.IsEnabled);
            return null;
        }

        return new TelegramBotClient(config.Token);
    }
}

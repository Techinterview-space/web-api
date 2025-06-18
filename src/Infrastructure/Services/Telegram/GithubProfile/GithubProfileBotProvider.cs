using Infrastructure.Services.Telegram.Salaries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Infrastructure.Services.Telegram.GithubProfile;

public class GithubProfileBotProvider : IGithubProfileBotProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GithubProfileBotProvider> _logger;

    public GithubProfileBotProvider(
        IConfiguration configuration,
        ILogger<GithubProfileBotProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public ITelegramBotClient CreateClient()
    {
        var enabled = _configuration["Telegram:GithubProfileBotEnable"]?.ToLowerInvariant();
        var parsedEnabled = bool.TryParse(enabled, out var isEnabled);

        var token = Environment.GetEnvironmentVariable("Telegram__GithubProfileBotToken");
        if (string.IsNullOrEmpty(token))
        {
            token = _configuration["Telegram:GithubProfileBotToken"];
        }

        if (!parsedEnabled ||
            !isEnabled ||
            string.IsNullOrEmpty(token))
        {
            _logger.LogWarning(
                "Github profile Telegram bot is disabled. Value {Value}. Parsed: {Parsed}",
                enabled,
                parsedEnabled);

            return null;
        }

        return new TelegramBotClient(token);
    }
}
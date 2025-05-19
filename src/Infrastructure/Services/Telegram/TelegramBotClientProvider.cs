using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Infrastructure.Services.Telegram;

public class TelegramBotClientProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramBotClientProvider> _logger;

    public TelegramBotClientProvider(
        IConfiguration configuration,
        ILogger<TelegramBotClientProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public ITelegramBotClient CreateClient()
    {
        var enabled = _configuration["Telegram:Enable"]?.ToLowerInvariant();
        var parsedEnabled = bool.TryParse(enabled, out var isEnabled);

        var token = Environment.GetEnvironmentVariable("Telegram__BotToken");
        if (string.IsNullOrEmpty(token))
        {
            token = _configuration["Telegram:BotToken"];
        }

        if (!parsedEnabled ||
            !isEnabled ||
            string.IsNullOrEmpty(token))
        {
            _logger.LogWarning(
                "Telegram bot is disabled. Value {Value}. Parsed: {Parsed}",
                enabled,
                parsedEnabled);

            return null;
        }

        return new TelegramBotClient(token);
    }
}
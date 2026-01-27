using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Infrastructure.Services.Telegram.Salaries;

public class SalariesTelegramBotClientProvider : ISalariesTelegramBotClientProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SalariesTelegramBotClientProvider> _logger;

    public SalariesTelegramBotClientProvider(
        IConfiguration configuration,
        ILogger<SalariesTelegramBotClientProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public ITelegramBotClient CreateClient()
    {
        var enabled = _configuration["Telegram:SalariesBotEnable"]?.ToLowerInvariant();
        var parsedEnabled = bool.TryParse(enabled, out var isEnabled);

        var token = Environment.GetEnvironmentVariable("Telegram__SalariesBotToken");
        if (string.IsNullOrEmpty(token))
        {
            token = _configuration["Telegram:SalariesBotToken"];
        }

        if (!parsedEnabled ||
            !isEnabled ||
            string.IsNullOrEmpty(token))
        {
            _logger.LogWarning(
                "Salaries Telegram bot is disabled. Value {Value}. Parsed: {Parsed}",
                enabled,
                parsedEnabled);

            return null;
        }

        return new TelegramBotClient(token);
    }
}
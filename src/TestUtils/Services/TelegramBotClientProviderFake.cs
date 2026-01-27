using System.Threading;
using Infrastructure.Services.Telegram;
using Infrastructure.Services.Telegram.Salaries;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace TestUtils.Services;

public class TelegramBotClientProviderFake : ISalariesTelegramBotClientProvider
{
    private readonly Mock<ITelegramBotClient> _telegramBotClient;

    public TelegramBotClientProviderFake()
    {
        _telegramBotClient = new Mock<ITelegramBotClient>();
    }

    public ITelegramBotClient CreateClient()
    {
        return _telegramBotClient.Object;
    }

    public Mock<ITelegramBotClient> GetMock()
    {
        return _telegramBotClient;
    }
}
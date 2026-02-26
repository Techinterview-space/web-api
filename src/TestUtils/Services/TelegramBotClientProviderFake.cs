using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Services.Telegram;
using Infrastructure.Services.Telegram.Salaries;
using Moq;
using Telegram.Bot;

namespace TestUtils.Services;

public class TelegramBotClientProviderFake : ISalariesTelegramBotClientProvider
{
    private readonly Mock<ITelegramBotClient> _telegramBotClient;

    public TelegramBotClientProviderFake()
    {
        _telegramBotClient = new Mock<ITelegramBotClient>();
    }

    public Task<ITelegramBotClient> CreateClientAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_telegramBotClient.Object);
    }

    public Mock<ITelegramBotClient> GetMock()
    {
        return _telegramBotClient;
    }
}
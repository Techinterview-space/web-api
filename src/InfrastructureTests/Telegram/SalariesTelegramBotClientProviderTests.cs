using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Telegram;
using Infrastructure.Services.Telegram;
using Infrastructure.Services.Telegram.Salaries;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot;
using Xunit;

namespace InfrastructureTests.Telegram;

public class SalariesTelegramBotClientProviderTests
{
    private readonly Mock<ITelegramBotConfigurationService> _configService;
    private readonly SalariesTelegramBotClientProvider _provider;

    public SalariesTelegramBotClientProviderTests()
    {
        _configService = new Mock<ITelegramBotConfigurationService>();
        var logger = new Mock<ILogger<SalariesTelegramBotClientProvider>>();
        _provider = new SalariesTelegramBotClientProvider(_configService.Object, logger.Object);
    }

    [Fact]
    public async Task CreateClientAsync_EnabledWithToken_ReturnsClient()
    {
        _configService
            .Setup(x => x.GetByBotTypeAsync(TelegramBotType.Salaries, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TelegramBotConfigurationCacheItem
            {
                BotType = TelegramBotType.Salaries,
                Token = "123456:ABCDEFGHIJ",
                IsEnabled = true,
            });

        var client = await _provider.CreateClientAsync(CancellationToken.None);

        Assert.NotNull(client);
        Assert.IsType<TelegramBotClient>(client);
    }

    [Fact]
    public async Task CreateClientAsync_Disabled_ReturnsNull()
    {
        _configService
            .Setup(x => x.GetByBotTypeAsync(TelegramBotType.Salaries, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TelegramBotConfigurationCacheItem
            {
                BotType = TelegramBotType.Salaries,
                Token = "123456:ABCDEFGHIJ",
                IsEnabled = false,
            });

        var client = await _provider.CreateClientAsync(CancellationToken.None);

        Assert.Null(client);
    }

    [Fact]
    public async Task CreateClientAsync_MissingToken_ReturnsNull()
    {
        _configService
            .Setup(x => x.GetByBotTypeAsync(TelegramBotType.Salaries, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TelegramBotConfigurationCacheItem
            {
                BotType = TelegramBotType.Salaries,
                Token = string.Empty,
                IsEnabled = true,
            });

        var client = await _provider.CreateClientAsync(CancellationToken.None);

        Assert.Null(client);
    }

    [Fact]
    public async Task CreateClientAsync_MissingRecord_ReturnsNull()
    {
        _configService
            .Setup(x => x.GetByBotTypeAsync(TelegramBotType.Salaries, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TelegramBotConfigurationCacheItem)null);

        var client = await _provider.CreateClientAsync(CancellationToken.None);

        Assert.Null(client);
    }
}

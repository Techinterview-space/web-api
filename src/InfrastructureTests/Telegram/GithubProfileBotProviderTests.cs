using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Telegram;
using Infrastructure.Services.Telegram;
using Infrastructure.Services.Telegram.GithubProfile;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot;
using Xunit;

namespace InfrastructureTests.Telegram;

public class GithubProfileBotProviderTests
{
    private readonly Mock<ITelegramBotConfigurationService> _configService;
    private readonly GithubProfileBotProvider _provider;

    public GithubProfileBotProviderTests()
    {
        _configService = new Mock<ITelegramBotConfigurationService>();
        var logger = new Mock<ILogger<GithubProfileBotProvider>>();
        _provider = new GithubProfileBotProvider(_configService.Object, logger.Object);
    }

    [Fact]
    public async Task CreateClientAsync_EnabledWithToken_ReturnsClient()
    {
        _configService
            .Setup(x => x.GetByBotTypeAsync(TelegramBotType.GithubProfile, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TelegramBotConfigurationCacheItem
            {
                BotType = TelegramBotType.GithubProfile,
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
            .Setup(x => x.GetByBotTypeAsync(TelegramBotType.GithubProfile, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TelegramBotConfigurationCacheItem
            {
                BotType = TelegramBotType.GithubProfile,
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
            .Setup(x => x.GetByBotTypeAsync(TelegramBotType.GithubProfile, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TelegramBotConfigurationCacheItem
            {
                BotType = TelegramBotType.GithubProfile,
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
            .Setup(x => x.GetByBotTypeAsync(TelegramBotType.GithubProfile, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TelegramBotConfigurationCacheItem)null);

        var client = await _provider.CreateClientAsync(CancellationToken.None);

        Assert.Null(client);
    }
}

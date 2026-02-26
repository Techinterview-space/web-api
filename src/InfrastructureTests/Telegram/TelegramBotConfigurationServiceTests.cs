using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Telegram;
using Infrastructure.Services.Telegram;
using Microsoft.Extensions.Logging;
using Moq;
using TestUtils.Db;
using Xunit;

namespace InfrastructureTests.Telegram;

public class TelegramBotConfigurationServiceTests
{
    [Fact]
    public async Task GetByBotTypeAsync_ExistingRecord_ReturnsCacheItem()
    {
        await using var context = new InMemoryDatabaseContext();

        await context.SaveAsync(
            new TelegramBotConfiguration(
                TelegramBotType.Salaries,
                "Salaries Bot",
                "123456:ABCDEFGHIJ",
                true,
                "salary_bot"));

        var service = CreateService(context);

        var result = await service.GetByBotTypeAsync(TelegramBotType.Salaries, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(TelegramBotType.Salaries, result.BotType);
        Assert.Equal("Salaries Bot", result.DisplayName);
        Assert.Equal("123456:ABCDEFGHIJ", result.Token);
        Assert.True(result.IsEnabled);
        Assert.Equal("salary_bot", result.BotUsername);
    }

    [Fact]
    public async Task GetByBotTypeAsync_MissingRecord_ReturnsNull()
    {
        await using var context = new InMemoryDatabaseContext();
        var service = CreateService(context);

        var result = await service.GetByBotTypeAsync(TelegramBotType.Salaries, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByBotTypeAsync_SecondCall_ReturnsCachedValue()
    {
        await using var context = new InMemoryDatabaseContext();

        await context.SaveAsync(
            new TelegramBotConfiguration(
                TelegramBotType.Salaries,
                "Salaries Bot",
                "token-1",
                true));

        var service = CreateService(context);

        var first = await service.GetByBotTypeAsync(TelegramBotType.Salaries, CancellationToken.None);
        var second = await service.GetByBotTypeAsync(TelegramBotType.Salaries, CancellationToken.None);

        Assert.Same(first, second);
    }

    [Fact]
    public async Task InvalidateCache_NextCallFetchesFromDb()
    {
        await using var context = new InMemoryDatabaseContext();

        var entity = await context.SaveAsync(
            new TelegramBotConfiguration(
                TelegramBotType.Salaries,
                "Old Name",
                "token-1",
                true));

        var service = CreateService(context);

        var first = await service.GetByBotTypeAsync(TelegramBotType.Salaries, CancellationToken.None);
        Assert.Equal("Old Name", first.DisplayName);

        entity.Update("New Name", true, string.Empty);
        await context.SaveChangesAsync();

        service.InvalidateCache(TelegramBotType.Salaries);

        var second = await service.GetByBotTypeAsync(TelegramBotType.Salaries, CancellationToken.None);
        Assert.Equal("New Name", second.DisplayName);
    }

    private static TelegramBotConfigurationService CreateService(InMemoryDatabaseContext context)
    {
        return new TelegramBotConfigurationService(
            context,
            new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions()),
            new Mock<ILogger<TelegramBotConfigurationService>>().Object);
    }
}

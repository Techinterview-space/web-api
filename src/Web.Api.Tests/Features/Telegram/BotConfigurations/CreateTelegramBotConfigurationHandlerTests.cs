using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Telegram;
using Domain.Validation.Exceptions;
using Infrastructure.Services.Telegram;
using Microsoft.EntityFrameworkCore;
using Moq;
using TestUtils.Db;
using Web.Api.Features.Telegram.BotConfigurations;
using Xunit;

namespace Web.Api.Tests.Features.Telegram.BotConfigurations;

public class CreateTelegramBotConfigurationHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_CreatesConfiguration()
    {
        await using var context = new InMemoryDatabaseContext();
        var configService = new Mock<ITelegramBotConfigurationService>();
        var handler = new CreateTelegramBotConfigurationHandler(context, configService.Object);

        var request = new CreateTelegramBotConfigurationRequest
        {
            BotType = TelegramBotType.Salaries,
            DisplayName = "Salaries Bot",
            Token = "123456:ABCDEFGHIJ",
            IsEnabled = true,
            BotUsername = "salary_bot",
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(TelegramBotType.Salaries, result.BotType);
        Assert.Equal("Salaries", result.BotTypeAsString);
        Assert.Equal("Salaries Bot", result.DisplayName);
        Assert.Equal("salary_bot", result.BotUsername);
        Assert.True(result.IsEnabled);
        Assert.True(result.HasToken);
        Assert.Equal("123456:ABC****", result.MaskedToken);

        var saved = await context.TelegramBotConfigurations.FirstOrDefaultAsync();
        Assert.NotNull(saved);
        Assert.Equal("123456:ABCDEFGHIJ", saved.Token);

        configService.Verify(x => x.InvalidateCache(TelegramBotType.Salaries), Times.Once);
    }

    [Fact]
    public async Task Handle_UndefinedBotType_ThrowsBadRequestException()
    {
        await using var context = new InMemoryDatabaseContext();
        var configService = new Mock<ITelegramBotConfigurationService>();
        var handler = new CreateTelegramBotConfigurationHandler(context, configService.Object);

        var request = new CreateTelegramBotConfigurationRequest
        {
            BotType = TelegramBotType.Undefined,
            DisplayName = "Bot",
            Token = "token",
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(request, CancellationToken.None));

        Assert.Contains("BotType", exception.Message);
    }

    [Fact]
    public async Task Handle_EmptyDisplayName_ThrowsBadRequestException()
    {
        await using var context = new InMemoryDatabaseContext();
        var configService = new Mock<ITelegramBotConfigurationService>();
        var handler = new CreateTelegramBotConfigurationHandler(context, configService.Object);

        var request = new CreateTelegramBotConfigurationRequest
        {
            BotType = TelegramBotType.Salaries,
            DisplayName = string.Empty,
            Token = "token",
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(request, CancellationToken.None));

        Assert.Contains("DisplayName", exception.Message);
    }

    [Fact]
    public async Task Handle_EmptyToken_ThrowsBadRequestException()
    {
        await using var context = new InMemoryDatabaseContext();
        var configService = new Mock<ITelegramBotConfigurationService>();
        var handler = new CreateTelegramBotConfigurationHandler(context, configService.Object);

        var request = new CreateTelegramBotConfigurationRequest
        {
            BotType = TelegramBotType.Salaries,
            DisplayName = "Bot",
            Token = string.Empty,
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(request, CancellationToken.None));

        Assert.Contains("Token", exception.Message);
    }

    [Fact]
    public async Task Handle_DuplicateBotType_ThrowsBadRequestException()
    {
        await using var context = new InMemoryDatabaseContext();
        var configService = new Mock<ITelegramBotConfigurationService>();

        await context.SaveAsync(
            new TelegramBotConfiguration(
                TelegramBotType.Salaries,
                "Existing",
                "existing-token",
                true));

        var handler = new CreateTelegramBotConfigurationHandler(context, configService.Object);

        var request = new CreateTelegramBotConfigurationRequest
        {
            BotType = TelegramBotType.Salaries,
            DisplayName = "Another",
            Token = "another-token",
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(request, CancellationToken.None));

        Assert.Contains("already exists", exception.Message);
    }
}

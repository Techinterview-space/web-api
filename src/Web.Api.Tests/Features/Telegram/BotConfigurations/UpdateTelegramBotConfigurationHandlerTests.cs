using System;
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

public class UpdateTelegramBotConfigurationHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_UpdatesConfiguration()
    {
        await using var context = new InMemoryDatabaseContext();
        var configService = new Mock<ITelegramBotConfigurationService>();

        var entity = await context.SaveAsync(
            new TelegramBotConfiguration(
                TelegramBotType.Salaries,
                "Old Name",
                "old-token-value",
                false));

        context.ChangeTracker.Clear();

        var handler = new UpdateTelegramBotConfigurationHandler(context, configService.Object);

        var command = new UpdateTelegramBotConfigurationCommand(
            entity.Id,
            new UpdateTelegramBotConfigurationRequest
            {
                DisplayName = "New Name",
                IsEnabled = true,
                BotUsername = "new_bot",
                Token = "new-token-value-here",
            });

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("New Name", result.DisplayName);
        Assert.True(result.IsEnabled);
        Assert.Equal("new_bot", result.BotUsername);
        Assert.True(result.HasToken);
        Assert.Equal("new-token-****", result.MaskedToken);

        configService.Verify(x => x.InvalidateCache(TelegramBotType.Salaries), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyToken_KeepsExistingToken()
    {
        await using var context = new InMemoryDatabaseContext();
        var configService = new Mock<ITelegramBotConfigurationService>();

        var entity = await context.SaveAsync(
            new TelegramBotConfiguration(
                TelegramBotType.Salaries,
                "Bot",
                "original-token-value",
                true));

        context.ChangeTracker.Clear();

        var handler = new UpdateTelegramBotConfigurationHandler(context, configService.Object);

        var command = new UpdateTelegramBotConfigurationCommand(
            entity.Id,
            new UpdateTelegramBotConfigurationRequest
            {
                DisplayName = "Updated Bot",
                IsEnabled = true,
                Token = string.Empty,
            });

        await handler.Handle(command, CancellationToken.None);

        var saved = await context.TelegramBotConfigurations.FirstAsync(x => x.Id == entity.Id);
        Assert.Equal("original-token-value", saved.Token);
    }

    [Fact]
    public async Task Handle_EmptyDisplayName_ThrowsBadRequestException()
    {
        await using var context = new InMemoryDatabaseContext();
        var configService = new Mock<ITelegramBotConfigurationService>();
        var handler = new UpdateTelegramBotConfigurationHandler(context, configService.Object);

        var command = new UpdateTelegramBotConfigurationCommand(
            Guid.NewGuid(),
            new UpdateTelegramBotConfigurationRequest
            {
                DisplayName = string.Empty,
            });

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Contains("DisplayName", exception.Message);
    }

    [Fact]
    public async Task Handle_NonExistingId_ThrowsNotFoundException()
    {
        await using var context = new InMemoryDatabaseContext();
        var configService = new Mock<ITelegramBotConfigurationService>();
        var handler = new UpdateTelegramBotConfigurationHandler(context, configService.Object);

        var command = new UpdateTelegramBotConfigurationCommand(
            Guid.NewGuid(),
            new UpdateTelegramBotConfigurationRequest
            {
                DisplayName = "Bot",
            });

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
    }
}

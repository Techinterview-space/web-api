using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Telegram;
using Domain.Validation.Exceptions;
using TestUtils.Db;
using Web.Api.Features.Telegram.BotConfigurations;
using Xunit;

namespace Web.Api.Tests.Features.Telegram.BotConfigurations;

public class GetTelegramBotConfigurationHandlerTests
{
    [Fact]
    public async Task HandleGetAll_TwoConfigurations_ReturnsBoth()
    {
        await using var context = new InMemoryDatabaseContext();

        await context.SaveAsync(
            new TelegramBotConfiguration(
                TelegramBotType.Salaries,
                "Salaries Bot",
                "token-1",
                true));

        await context.SaveAsync(
            new TelegramBotConfiguration(
                TelegramBotType.GithubProfile,
                "Github Bot",
                "token-2",
                false));

        var handler = new GetTelegramBotConfigurationsHandler(context);

        var result = await handler.Handle(
            new GetTelegramBotConfigurationsQuery(),
            CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(TelegramBotType.Salaries, result[0].BotType);
        Assert.Equal(TelegramBotType.GithubProfile, result[1].BotType);
    }

    [Fact]
    public async Task HandleGetAll_Empty_ReturnsEmptyList()
    {
        await using var context = new InMemoryDatabaseContext();
        var handler = new GetTelegramBotConfigurationsHandler(context);

        var result = await handler.Handle(
            new GetTelegramBotConfigurationsQuery(),
            CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleGetById_ExistingId_ReturnsDto()
    {
        await using var context = new InMemoryDatabaseContext();

        var entity = await context.SaveAsync(
            new TelegramBotConfiguration(
                TelegramBotType.Salaries,
                "Salaries Bot",
                "123456:ABCDEFGHIJ",
                true,
                "salary_bot"));

        var handler = new GetTelegramBotConfigurationByIdHandler(context);

        var result = await handler.Handle(
            new GetTelegramBotConfigurationByIdQuery(entity.Id),
            CancellationToken.None);

        Assert.Equal(entity.Id, result.Id);
        Assert.Equal("Salaries Bot", result.DisplayName);
        Assert.Equal("salary_bot", result.BotUsername);
        Assert.True(result.HasToken);
        Assert.Equal("123456:ABC****", result.MaskedToken);
    }

    [Fact]
    public async Task HandleGetById_NonExistingId_ThrowsNotFoundException()
    {
        await using var context = new InMemoryDatabaseContext();
        var handler = new GetTelegramBotConfigurationByIdHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                new GetTelegramBotConfigurationByIdQuery(Guid.NewGuid()),
                CancellationToken.None));
    }
}

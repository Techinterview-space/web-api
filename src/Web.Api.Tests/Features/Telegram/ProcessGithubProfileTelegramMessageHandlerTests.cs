using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Services.Github;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TestUtils.Db;
using Web.Api.Features.Telegram;
using Web.Api.Features.Telegram.GithubProfiles;
using Xunit;

namespace Web.Api.Tests.Features.Telegram;

public class ProcessGithubProfileTelegramMessageHandlerTests
{
    [Fact]
    public async Task Handle_InlineQuery_AnswersInlineQuery()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();
        
        var loggerMock = new Mock<ILogger<ProcessGithubProfileTelegramMessageHandler>>();
        var githubClientServiceMock = new Mock<GithubClientService>(Mock.Of<ILogger<GithubClientService>>());
        var githubGraphQLServiceMock = new Mock<IGithubGraphQLService>();
        var configurationMock = new Mock<IConfiguration>();
        var botClientMock = new Mock<ITelegramBotClient>();
        
        var handler = new ProcessGithubProfileTelegramMessageHandler(
            loggerMock.Object,
            githubClientServiceMock.Object,
            githubGraphQLServiceMock.Object,
            context,
            configurationMock.Object);

        var inlineQuery = new InlineQuery
        {
            Id = "test-query-id",
            From = new User { Id = 123, Username = "testuser" },
            Query = "octocat"
        };

        var update = new Update
        {
            Type = UpdateType.InlineQuery,
            InlineQuery = inlineQuery
        };

        var command = new ProcessTelegramMessageCommand(botClientMock.Object, update);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(result);
        botClientMock.Verify(x => x.AnswerInlineQuery(
            It.IsAny<string>(),
            It.IsAny<Telegram.Bot.Types.InlineQueryResults.InlineQueryResult[]>(),
            It.IsAny<int>(),
            null,
            null,
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MessageSentByBot_LogsInlineClick()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();
        
        var loggerMock = new Mock<ILogger<ProcessGithubProfileTelegramMessageHandler>>();
        var githubClientServiceMock = new Mock<GithubClientService>(Mock.Of<ILogger<GithubClientService>>());
        var githubGraphQLServiceMock = new Mock<IGithubGraphQLService>();
        var configurationMock = new Mock<IConfiguration>();
        var botClientMock = new Mock<ITelegramBotClient>();

        // Setup bot user
        var botUser = new User { Id = 456, Username = "github_profile_bot" };
        botClientMock.Setup(x => x.GetMe(It.IsAny<CancellationToken>()))
            .ReturnsAsync(botUser);
        
        var handler = new ProcessGithubProfileTelegramMessageHandler(
            loggerMock.Object,
            githubClientServiceMock.Object,
            githubGraphQLServiceMock.Object,
            context,
            configurationMock.Object);

        var message = new Message
        {
            MessageId = 1,
            From = new User { Id = 123, Username = "testuser" },
            Chat = new Chat { Id = 789, Type = ChatType.Private },
            Text = "@octocat",
            ViaBot = botUser
        };

        var update = new Update
        {
            Type = UpdateType.Message,
            Message = message
        };

        var command = new ProcessTelegramMessageCommand(botClientMock.Object, update);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(result);
        
        // Verify that TelegramInlineReply was added to context
        var inlineReply = await context.TelegramInlineReplies.FirstOrDefaultAsync();
        Assert.NotNull(inlineReply);
        Assert.Equal("testuser", inlineReply.Username);
        Assert.Equal(123L, inlineReply.UserId);
        Assert.Equal(789L, inlineReply.ChatId);
    }
}
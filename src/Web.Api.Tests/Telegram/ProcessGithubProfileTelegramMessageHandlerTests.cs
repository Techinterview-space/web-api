using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Web.Api.Features.Telegram.GithubProfiles;
using Xunit;

namespace Web.Api.Tests.Telegram
{
    public class ProcessGithubProfileTelegramMessageHandlerTests
    {
        [Fact]
        public async Task Handle_ValidUsername_ReturnsProfileInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProcessGithubProfileTelegramMessageHandler>>();
            var botClientMock = new Mock<ITelegramBotClient>();
            var handler = new ProcessGithubProfileTelegramMessageHandler(loggerMock.Object);

            var message = new Message
            {
                MessageId = 1,
                Text = "octocat",
                Chat = new Chat { Id = 1 }
            };

            var update = new Update
            {
                Message = message
            };

            var command = new ProcessTelegramMessageCommand(botClientMock.Object, update);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Hello", result);
            Assert.Contains("GitHub profile", result);
        }

        [Fact]
        public async Task Handle_EmptyMessage_ReturnsEmptyString()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProcessGithubProfileTelegramMessageHandler>>();
            var botClientMock = new Mock<ITelegramBotClient>();
            var handler = new ProcessGithubProfileTelegramMessageHandler(loggerMock.Object);

            var update = new Update
            {
                Message = null
            };

            var command = new ProcessTelegramMessageCommand(botClientMock.Object, update);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task Handle_HelpCommand_ReturnsHelpMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProcessGithubProfileTelegramMessageHandler>>();
            var botClientMock = new Mock<ITelegramBotClient>();
            var handler = new ProcessGithubProfileTelegramMessageHandler(loggerMock.Object);

            var message = new Message
            {
                MessageId = 1,
                Text = "/help",
                Chat = new Chat { Id = 1 }
            };

            var update = new Update
            {
                Message = message
            };

            var command = new ProcessTelegramMessageCommand(botClientMock.Object, update);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Contains("Welcome to the GitHub Profile Bot", result);
        }
    }
}
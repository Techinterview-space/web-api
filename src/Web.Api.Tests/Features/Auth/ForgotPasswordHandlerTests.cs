using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Users;
using Domain.Enums;
using Infrastructure.Emails.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Auth.Handlers;
using Web.Api.Features.Auth.Requests;
using Xunit;

namespace Web.Api.Tests.Features.Auth;

public class ForgotPasswordHandlerTests
{
    [Fact]
    public async Task Handle_UserExists_ReturnsSuccessAndSendsEmail()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        user.SetPassword("hashed_password");
        await context.TrySaveChangesAsync();

        var emailService = new Mock<ITechinterviewEmailService>();
        emailService
            .Setup(x => x.SendPasswordResetAsync(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(context, emailService: emailService.Object);
        var request = new ForgotPasswordRequest { Email = user.Email };

        context.ChangeTracker.Clear();

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.True(result.Success);
        emailService.Verify(
            x => x.SendPasswordResetAsync(
                It.Is<User>(u => u.Id == user.Id),
                It.Is<string>(url => url.Contains("/reset-password?token=")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UserWithEmptyPasswordHash_ReturnsSuccessAndSendsEmail()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var emailService = new Mock<ITechinterviewEmailService>();
        emailService
            .Setup(x => x.SendPasswordResetAsync(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(context, emailService: emailService.Object);
        var request = new ForgotPasswordRequest { Email = user.Email };

        context.ChangeTracker.Clear();

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.True(result.Success);
        emailService.Verify(
            x => x.SendPasswordResetAsync(
                It.Is<User>(u => u.Id == user.Id),
                It.Is<string>(url => url.Contains("/reset-password?token=")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsSuccessWithoutSendingEmail()
    {
        await using var context = new InMemoryDatabaseContext();

        var emailService = new Mock<ITechinterviewEmailService>();
        var handler = CreateHandler(context, emailService: emailService.Object);
        var request = new ForgotPasswordRequest { Email = "nonexistent@example.com" };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.True(result.Success);
        emailService.Verify(
            x => x.SendPasswordResetAsync(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_EmailCaseInsensitive_FindsUserAndSendsEmail()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var emailService = new Mock<ITechinterviewEmailService>();
        emailService
            .Setup(x => x.SendPasswordResetAsync(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(context, emailService: emailService.Object);
        var request = new ForgotPasswordRequest { Email = user.Email.ToUpperInvariant() };

        context.ChangeTracker.Clear();

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.True(result.Success);
        emailService.Verify(
            x => x.SendPasswordResetAsync(
                It.Is<User>(u => u.Id == user.Id),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UserExists_SetsPasswordResetTokenOnUser()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var emailService = new Mock<ITechinterviewEmailService>();
        emailService
            .Setup(x => x.SendPasswordResetAsync(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(context, emailService: emailService.Object);
        var request = new ForgotPasswordRequest { Email = user.Email };

        context.ChangeTracker.Clear();

        await handler.Handle(request, CancellationToken.None);

        var updatedUser = await context.Users.FirstAsync(u => u.Id == user.Id);
        Assert.NotNull(updatedUser.PasswordResetToken);
        Assert.NotNull(updatedUser.PasswordResetTokenExpiresAt);
        Assert.True(updatedUser.PasswordResetTokenExpiresAt > System.DateTimeOffset.UtcNow);
    }

    private static ForgotPasswordHandler CreateHandler(
        InMemoryDatabaseContext context,
        ITechinterviewEmailService emailService = null)
    {
        emailService ??= Mock.Of<ITechinterviewEmailService>();

        var config = new Mock<IConfiguration>();
        config
            .Setup(x => x[It.Is<string>(k => k == "Frontend:BaseUrl")])
            .Returns("https://localhost:3000");

        return new ForgotPasswordHandler(context, emailService, config.Object);
    }
}

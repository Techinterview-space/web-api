using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Users;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Emails.Contracts;
using Microsoft.Extensions.Configuration;
using Moq;
using TestUtils.Db;
using Web.Api.Features.Auth.Handlers;
using Web.Api.Features.Auth.Requests;
using Xunit;

namespace Web.Api.Tests.Features.Auth;

public class RegisterHandlerAntiBotTests
{
    [Fact]
    public async Task Handle_HoneypotFilled_ThrowsBadRequestException()
    {
        await using var context = new InMemoryDatabaseContext();

        var handler = CreateHandler(context);

        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "SecurePass123",
            FirstName = "John",
            LastName = "Doe",
            Website = "http://spam.com",
            FormDurationSeconds = 10,
        };

        await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_HoneypotEmpty_DoesNotRejectOnHoneypot()
    {
        await using var context = new InMemoryDatabaseContext();

        var emailService = new Mock<ITechinterviewEmailService>();
        emailService
            .Setup(x => x.SendEmailVerificationAsync(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(context, emailService: emailService.Object);

        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "SecurePass123",
            FirstName = "John",
            LastName = "Doe",
            Website = string.Empty,
            FormDurationSeconds = 10,
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task Handle_TimingTooFast_ThrowsBadRequestException()
    {
        await using var context = new InMemoryDatabaseContext();

        var handler = CreateHandler(context);

        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "SecurePass123",
            FirstName = "John",
            LastName = "Doe",
            FormDurationSeconds = 1,
        };

        await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_TimingAtThreshold_DoesNotRejectOnTiming()
    {
        await using var context = new InMemoryDatabaseContext();

        var emailService = new Mock<ITechinterviewEmailService>();
        emailService
            .Setup(x => x.SendEmailVerificationAsync(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(context, emailService: emailService.Object);

        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "SecurePass123",
            FirstName = "John",
            LastName = "Doe",
            FormDurationSeconds = 2,
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task Handle_TimingNull_DoesNotRejectOnTiming()
    {
        await using var context = new InMemoryDatabaseContext();

        var emailService = new Mock<ITechinterviewEmailService>();
        emailService
            .Setup(x => x.SendEmailVerificationAsync(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(context, emailService: emailService.Object);

        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "SecurePass123",
            FirstName = "John",
            LastName = "Doe",
            FormDurationSeconds = null,
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.True(result.Success);
    }

    private static RegisterHandler CreateHandler(
        InMemoryDatabaseContext context,
        IPasswordHasher passwordHasher = null,
        ITechinterviewEmailService emailService = null)
    {
        passwordHasher ??= Mock.Of<IPasswordHasher>(
            x => x.Hash(It.IsAny<string>()) == "hashed_password");

        emailService ??= Mock.Of<ITechinterviewEmailService>();

        var config = new Mock<IConfiguration>();
        config
            .Setup(x => x[It.Is<string>(k => k == "Frontend:BaseUrl")])
            .Returns("https://localhost:3000");

        return new RegisterHandler(context, passwordHasher, emailService, config.Object);
    }
}

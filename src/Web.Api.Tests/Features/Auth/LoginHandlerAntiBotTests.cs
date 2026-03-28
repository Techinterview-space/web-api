using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Auth;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Jwt;
using Moq;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Auth.Handlers;
using Web.Api.Features.Auth.Requests;
using Xunit;

namespace Web.Api.Tests.Features.Auth;

public class LoginHandlerAntiBotTests
{
    [Fact]
    public async Task Handle_HoneypotFilled_ThrowsUnauthorizedException()
    {
        await using var context = new InMemoryDatabaseContext();

        var handler = new LoginHandler(
            context,
            Mock.Of<IPasswordHasher>(),
            Mock.Of<IJwtTokenService>());

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123",
            Website = "http://spam.com",
            FormDurationSeconds = 10,
        };

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_HoneypotEmpty_DoesNotRejectOnHoneypot()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        user.SetPassword("hashed");
        await context.TrySaveChangesAsync();

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        var jwtService = new Mock<IJwtTokenService>();
        jwtService
            .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns("access_token");
        jwtService
            .Setup(x => x.GenerateRefreshToken(It.IsAny<long>(), It.IsAny<string>()))
            .Returns(new RefreshToken(user.Id, "refresh_token", DateTimeOffset.UtcNow.AddDays(10)));

        var handler = new LoginHandler(context, passwordHasher.Object, jwtService.Object);

        var request = new LoginRequest
        {
            Email = user.Email,
            Password = "password123",
            Website = string.Empty,
            FormDurationSeconds = 10,
        };

        context.ChangeTracker.Clear();

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("access_token", result.AccessToken);
    }

    [Fact]
    public async Task Handle_TimingTooFast_ThrowsUnauthorizedException()
    {
        await using var context = new InMemoryDatabaseContext();

        var handler = new LoginHandler(
            context,
            Mock.Of<IPasswordHasher>(),
            Mock.Of<IJwtTokenService>());

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123",
            FormDurationSeconds = 0,
        };

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_TimingAtThreshold_DoesNotRejectOnTiming()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        user.SetPassword("hashed");
        await context.TrySaveChangesAsync();

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        var jwtService = new Mock<IJwtTokenService>();
        jwtService
            .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns("access_token");
        jwtService
            .Setup(x => x.GenerateRefreshToken(It.IsAny<long>(), It.IsAny<string>()))
            .Returns(new RefreshToken(user.Id, "refresh_token", DateTimeOffset.UtcNow.AddDays(10)));

        var handler = new LoginHandler(context, passwordHasher.Object, jwtService.Object);

        var request = new LoginRequest
        {
            Email = user.Email,
            Password = "password123",
            FormDurationSeconds = 1,
        };

        context.ChangeTracker.Clear();

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Handle_TimingNull_DoesNotRejectOnTiming()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        user.SetPassword("hashed");
        await context.TrySaveChangesAsync();

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        var jwtService = new Mock<IJwtTokenService>();
        jwtService
            .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns("access_token");
        jwtService
            .Setup(x => x.GenerateRefreshToken(It.IsAny<long>(), It.IsAny<string>()))
            .Returns(new RefreshToken(user.Id, "refresh_token", DateTimeOffset.UtcNow.AddDays(10)));

        var handler = new LoginHandler(context, passwordHasher.Object, jwtService.Object);

        var request = new LoginRequest
        {
            Email = user.Email,
            Password = "password123",
            FormDurationSeconds = null,
        };

        context.ChangeTracker.Clear();

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.NotNull(result);
    }
}

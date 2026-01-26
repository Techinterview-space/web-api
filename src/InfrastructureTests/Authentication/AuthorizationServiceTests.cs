using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Xunit;

namespace InfrastructureTests.Authentication;

public class AuthorizationServiceTests
{
    [Fact]
    public async Task CurrentUserAsync_NewUser_Creates()
    {
        await using var context = new SqliteContext();
        var target = new AuthorizationService(
            new FakeHttpContext(
                new FakeCurrentUser(
                    userId: "42",
                    role: Role.Interviewer,
                    firstName: "John",
                    lastName: "Smith")),
            context,
            new Mock<ILogger<AuthorizationService>>().Object);

        Assert.False(await context.Users.AnyAsync());

        var currentUser = await target.GetCurrentUserOrNullAsync();
        Assert.Equal(1, await context.Users.CountAsync());

        Assert.Equal("42", currentUser.IdentityId);
        Assert.Equal("John", currentUser.FirstName);
        Assert.Equal("Smith", currentUser.LastName);
        Assert.Single(currentUser.UserRoles);
        Assert.Equal(Role.Interviewer, currentUser.UserRoles.Single().RoleId);
    }

    [Fact]
    public async Task CurrentUserAsync_ExistingUser_NullIdentity_Ok()
    {
        await using var context = new SqliteContext();
        var oldUser = await new UserFake(
                role: Role.Interviewer,
                firstName: "John",
                lastName: "Smith")
            .WithIdentity(null)
            .PleaseAsync(context);

        Assert.Null(oldUser.IdentityId);
        Assert.False(oldUser.EmailConfirmed);

        var fakeUser = new FakeCurrentUser(oldUser)
            .WithUserId("google-oauth2|1234");

        var target = new AuthorizationService(
            new FakeHttpContext(fakeUser),
            context,
            new Mock<ILogger<AuthorizationService>>().Object);

        Assert.Equal(1, await context.Users.CountAsync());
        var currentUser = await target.GetCurrentUserOrNullAsync();
        Assert.Equal(1, await context.Users.CountAsync());

        Assert.Equal(oldUser.Id, currentUser.Id);
        Assert.NotNull(currentUser.IdentityId);
        Assert.True(currentUser.EmailConfirmed);
        Assert.Equal("John", currentUser.FirstName);
        Assert.Equal("Smith", currentUser.LastName);
        Assert.Single(currentUser.UserRoles);
        Assert.Equal(Role.Interviewer, currentUser.UserRoles.Single().RoleId);
    }

    [Fact]
    public async Task CurrentUserAsync_ExistingUser_DoesntCreateNewOne()
    {
        await using var context = new SqliteContext();
        var oldUser = await new UserFake(
                role: Role.Interviewer,
                firstName: "John",
                lastName: "Smith")
            .WithIdentity($"google-oauth2|{Guid.NewGuid():N}")
            .PleaseAsync(context);

        Assert.NotNull(oldUser.IdentityId);
        Assert.False(oldUser.EmailConfirmed);

        var fakeUser = new FakeCurrentUser(oldUser)
            .WithUserId(oldUser.IdentityId);

        var target = new AuthorizationService(
            new FakeHttpContext(fakeUser),
            context,
            new Mock<ILogger<AuthorizationService>>().Object);

        Assert.Equal(1, await context.Users.CountAsync());
        var currentUser = await target.GetCurrentUserOrNullAsync();
        Assert.Equal(1, await context.Users.CountAsync());

        Assert.Equal(oldUser.Id, currentUser.Id);
        Assert.NotNull(currentUser.IdentityId);
        Assert.True(currentUser.EmailConfirmed);
        Assert.Equal("John", currentUser.FirstName);
        Assert.Equal("Smith", currentUser.LastName);
        Assert.Single(currentUser.UserRoles);
        Assert.Equal(Role.Interviewer, currentUser.UserRoles.Single().RoleId);
    }

    [Theory]
    [InlineData(CurrentUser.GoogleOAuth2Prefix)]
    [InlineData(CurrentUser.GithubPrefix)]
    public async Task CurrentUserAsync_ExistingUserWithSocial_DifferentIdentity_Error(
        string socialPrefix)
    {
        await using var context = new SqliteContext();
        var oldUser = await new UserFake(
                role: Role.Interviewer,
                firstName: "John",
                lastName: "Smith")
            .WithIdentity($"{socialPrefix}{Guid.NewGuid():N}")
            .PleaseAsync(context);

        Assert.NotNull(oldUser.IdentityId);
        Assert.False(oldUser.EmailConfirmed);

        var fakeUser = new FakeCurrentUser(oldUser)
            .WithUserId($"auth2|{Guid.NewGuid():N}");

        var target = new AuthorizationService(
            new FakeHttpContext(fakeUser),
            context,
            new Mock<ILogger<AuthorizationService>>().Object);

        Assert.Equal(1, await context.Users.CountAsync());
        await Assert.ThrowsAsync<AuthenticationException>(() => target.GetCurrentUserOrNullAsync());

        Assert.Equal(1, await context.Users.CountAsync());
    }

    [Theory]
    [InlineData(CurrentUser.GoogleOAuth2Prefix)]
    [InlineData(CurrentUser.GithubPrefix)]
    public async Task CurrentUserAsync_ExistingUserWithAuth0Identity_DifferentSocialIdentity_Ok(
        string socialPrefix)
    {
        await using var context = new SqliteContext();
        var oldUser = await new UserFake(
                role: Role.Interviewer,
                firstName: "John",
                lastName: "Smith")
            .WithIdentity($"auth0|{Guid.NewGuid():N}")
            .PleaseAsync(context);

        Assert.NotNull(oldUser.IdentityId);
        Assert.False(oldUser.EmailConfirmed);

        var fakeUser = new FakeCurrentUser(oldUser)
            .WithUserId($"{socialPrefix}{Guid.NewGuid():N}");

        var target = new AuthorizationService(
            new FakeHttpContext(fakeUser),
            context,
            new Mock<ILogger<AuthorizationService>>().Object);

        Assert.Equal(1, await context.Users.CountAsync());
        var user = await target.GetCurrentUserOrNullAsync();

        Assert.Equal(1, await context.Users.CountAsync());
        Assert.Equal(oldUser.Id, user.Id);
    }

    [Theory]
    [InlineData(CurrentUser.GoogleOAuth2Prefix)]
    [InlineData(CurrentUser.GithubPrefix)]
    public async Task CurrentUserAsync_ExistingAdminWithAuth0Identity_DifferentSocialIdentity_Error(
        string socialPrefix)
    {
        await using var context = new SqliteContext();
        var oldUser = await new UserFake(
                role: Role.Admin,
                firstName: "John",
                lastName: "Smith")
            .WithIdentity($"auth0|{Guid.NewGuid():N}")
            .PleaseAsync(context);

        Assert.NotNull(oldUser.IdentityId);
        Assert.False(oldUser.EmailConfirmed);

        var fakeUser = new FakeCurrentUser(oldUser)
            .WithUserId($"{socialPrefix}{Guid.NewGuid():N}");

        var target = new AuthorizationService(
            new FakeHttpContext(fakeUser),
            context,
            new Mock<ILogger<AuthorizationService>>().Object);

        Assert.Equal(1, await context.Users.CountAsync());
        await Assert.ThrowsAsync<AuthenticationException>(() => target.GetCurrentUserOrNullAsync());

        Assert.Equal(1, await context.Users.CountAsync());
    }

    [Fact]
    public async Task CurrentUserAsync_ExistingUserWithAuth0Identity_DifferentAuth0Identity_Error()
    {
        await using var context = new SqliteContext();
        var oldUser = await new UserFake(
                role: Role.Interviewer,
                firstName: "John",
                lastName: "Smith")
            .WithIdentity($"auth2|{Guid.NewGuid():N}")
            .PleaseAsync(context);

        Assert.NotNull(oldUser.IdentityId);
        Assert.False(oldUser.EmailConfirmed);

        var fakeUser = new FakeCurrentUser(oldUser)
            .WithUserId($"auth2|{Guid.NewGuid():N}");

        var target = new AuthorizationService(
            new FakeHttpContext(fakeUser),
            context,
            new Mock<ILogger<AuthorizationService>>().Object);

        Assert.Equal(1, await context.Users.CountAsync());
        await Assert.ThrowsAsync<AuthenticationException>(() => target.GetCurrentUserOrNullAsync());

        Assert.Equal(1, await context.Users.CountAsync());
    }

    [Fact]
    public async Task CurrentUserAsync_ExistingUser_UserChangedEmailInSocial_ChangedEmail()
    {
        await using var context = new SqliteContext();
        var oldUser = await new UserFake(
                role: Role.Interviewer,
                firstName: "John",
                lastName: "Smith")
            .WithIdentity($"github|{Guid.NewGuid():N}")
            .PleaseAsync(context);

        Assert.NotNull(oldUser.IdentityId);
        Assert.False(oldUser.EmailConfirmed);

        var fakeUser = new FakeCurrentUser(oldUser)
            .WithUserId(oldUser.IdentityId)
            .WithEmail($"{Guid.NewGuid():N}@github.com");

        Assert.NotEqual(oldUser.Email, fakeUser.Email);

        var target = new AuthorizationService(
            new FakeHttpContext(fakeUser),
            context,
            new Mock<ILogger<AuthorizationService>>().Object);

        Assert.Equal(1, await context.Users.CountAsync());
        var currentUser = await target.GetCurrentUserOrNullAsync();
        Assert.Equal(1, await context.Users.CountAsync());

        Assert.Equal(oldUser.Id, currentUser.Id);

        Assert.NotNull(currentUser.IdentityId);
        Assert.Equal(fakeUser.UserId, currentUser.IdentityId);

        Assert.True(currentUser.EmailConfirmed);
        Assert.Equal("John", currentUser.FirstName);
        Assert.Equal("Smith", currentUser.LastName);
        Assert.Single(currentUser.UserRoles);
        Assert.Equal(Role.Interviewer, currentUser.UserRoles.Single().RoleId);
        Assert.Equal(fakeUser.Email, currentUser.Email);
    }
}
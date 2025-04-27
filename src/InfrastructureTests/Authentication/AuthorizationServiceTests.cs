using System.Linq;
using System.Threading.Tasks;
using Domain.Enums;
using Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Xunit;

namespace InfrastructureTests.Authentication;

public class AuthorizationServiceTests
{
    [Fact]
    public async Task CurrentUserAsync_NewUser_CreatesAsync()
    {
        await using var context = new SqliteContext();
        var target = new AuthorizationService(
            new FakeHttpContext(
                new FakeCurrentUser(
                    id: "42",
                    role: Role.Interviewer,
                    firstName: "John",
                    lastName: "Smith")),
            context);

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
    public async Task CurrentUserAsync_NotNewUser_DoesntCreateAsync()
    {
        await using var context = new SqliteContext();
        var oldUser = await new UserFake(
                role: Role.Interviewer,
                firstName: "John",
                lastName: "Smith")
            .PleaseAsync(context);
        Assert.Null(oldUser.IdentityId);
        Assert.False(oldUser.EmailConfirmed);

        var target = new AuthorizationService(
            new FakeHttpContext(new FakeCurrentUser(oldUser)),
            context);

        Assert.Equal(1, await context.Users.CountAsync());
        var currentUser = await target.GetCurrentUserOrNullAsync();
        Assert.Equal(1, await context.Users.CountAsync());

        Assert.Equal(oldUser.Id, currentUser.Id);
        Assert.NotNull(currentUser.IdentityId);
        Assert.True(currentUser.EmailConfirmed);
        Assert.Equal(oldUser.Id.ToString(), currentUser.IdentityId);
        Assert.Equal("John", currentUser.FirstName);
        Assert.Equal("Smith", currentUser.LastName);
        Assert.Single(currentUser.UserRoles);
        Assert.Equal(Role.Interviewer, currentUser.UserRoles.Single().RoleId);
    }
}
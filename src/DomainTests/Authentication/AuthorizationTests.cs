using System.Linq;
using System.Threading.Tasks;
using Domain.Authentication;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Xunit;

namespace DomainTests.Authentication;

public class AuthorizationTests
{
    [Fact]
    public async Task CurrentUserAsync_NewUser_CreatesAsync()
    {
        await using var context = new SqliteContext();
        var target = new Authorization(
            new FakeHttpContext(
                new FakeCurrentUser(
                    id: "42",
                    role: Role.Interviewer,
                    firstName: "John",
                    lastName: "Smith")),
            context);

        Assert.False(await context.Users.AnyAsync());

        var currentUser = await target.CurrentUserAsync();
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
        var oldUser = await new FakeUser(
                role: Role.Interviewer,
                firstName: "John",
                lastName: "Smith")
            .PleaseAsync(context);
        Assert.Null(oldUser.IdentityId);
        Assert.False(oldUser.EmailConfirmed);

        var target = new Authorization(
            new FakeHttpContext(new FakeCurrentUser(oldUser)),
            context);

        Assert.Equal(1, await context.Users.CountAsync());
        var currentUser = await target.CurrentUserAsync();
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

    [Fact]
    public async Task MyOrganizationsAsync_HasOrganizations_ReturnAsync()
    {
        await using var context = new SqliteContext();
        var organization1 = await new FakeOrganization().PleaseAsync(context);
        var organization2 = await new FakeOrganization().PleaseAsync(context);
        var organization3 = await new FakeOrganization().PleaseAsync(context);

        var user = await new FakeUser(
                role: Role.Interviewer,
                firstName: "John",
                lastName: "Smith")
            .AttachToOrganization(organization1)
            .AttachToOrganization(organization2)
            .AttachToOrganization(organization3)
            .PleaseAsync(context);

        var target = new Authorization(
            new FakeHttpContext(new FakeCurrentUser(user)),
            context);

        var organizations = await target.MyOrganizationsAsync();

        var expected = new[]
        {
            organization1.Id,
            organization2.Id,
            organization3.Id,
        };

        Assert.Equal(expected, organizations);
    }
}
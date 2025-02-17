using System.Threading.Tasks;
using Domain.Enums;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Accounts;
using Xunit;

namespace Web.Api.Tests.Controllers.Accounts;

public class AccountControllerTests
{
    [Fact]
    public async Task GetMe_UserHimself_ReturnsSalaries()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new FakeUser(Role.Interviewer)
            .PleaseAsync(context);

        var salary1 = await new UserSalaryFake(user, 400_000)
            .PleaseAsync(context);

        var salary2 = await new UserSalaryFake(user, 500_000)
            .PleaseAsync(context);

        var controller = new AccountController(
            new FakeAuth(user),
            context);

        context.ChangeTracker.Clear();
        var result = await controller.Me(default);

        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(2, result.Salaries.Count);
        Assert.Equal(salary1.Value, result.Salaries[0].Value);
        Assert.Equal(salary2.Value, result.Salaries[1].Value);
    }

    [Fact]
    public async Task GetMe_OtherUser_OtherUserProfile()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new FakeUser(Role.Interviewer)
            .PleaseAsync(context);

        var user2 = await new FakeUser(Role.Admin)
            .PleaseAsync(context);

        var salary1 = await new UserSalaryFake(user1, 400_000)
            .PleaseAsync(context);

        var salary2 = await new UserSalaryFake(user1, 500_000)
            .PleaseAsync(context);

        var controller = new AccountController(
            new FakeAuth(user2),
            context);

        context.ChangeTracker.Clear();
        var result = await controller.Me(default);

        Assert.Equal(user2.Id, result.Id);
        Assert.Equal(user2.Email, result.Email);
        Assert.Empty(result.Salaries);
    }
}
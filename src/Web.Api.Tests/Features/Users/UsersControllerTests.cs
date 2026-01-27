using System.Threading.Tasks;
using Domain.Enums;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Users;
using Xunit;

namespace Web.Api.Tests.Features.Users;

public class UsersControllerTests
{
    [Fact]
    public async Task GetUser_UserHimself_ReturnsSalaries()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var user2 = await new UserFake(Role.Admin).PleaseAsync(context);

        var salary1 = await new UserSalaryFake(user1, 400_000).PleaseAsync(context);
        var salary2 = await new UserSalaryFake(user1, 600_000).PleaseAsync(context);

        var controller = new UsersController(
            new FakeAuth(user1),
            context);

        context.ChangeTracker.Clear();
        var result = await controller.GetUser(user1.Id, default);
        Assert.Equal(user1.Id, result.Id);
    }

    [Fact]
    public async Task GetUser_Admin_ReturnsNoSalaries()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var user2 = await new UserFake(Role.Admin).PleaseAsync(context);

        var salary1 = await new UserSalaryFake(user1, 400_000).PleaseAsync(context);
        var salary2 = await new UserSalaryFake(user1, 600_000).PleaseAsync(context);

        var controller = new UsersController(
            new FakeAuth(user2),
            context);

        context.ChangeTracker.Clear();
        var result = await controller.GetUser(user1.Id, default);
        Assert.Equal(user1.Id, result.Id);
    }
}
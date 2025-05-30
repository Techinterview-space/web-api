using System.Threading.Tasks;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Accounts;
using Xunit;

namespace Web.Api.Tests.Controllers.Accounts;

public class AccountControllerTests
{
    [Fact]
    public async Task GetMySalaries_UserHimself_ReturnsSalaries()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer)
            .PleaseAsync(context);

        var salary1 = await new UserSalaryFake(user, 400_000)
            .PleaseAsync(context);

        var salary2 = await new UserSalaryFake(user, 500_000)
            .PleaseAsync(context);

        var controller = new AccountController(
            new FakeAuth(user),
            context,
            new Mock<ILogger<AccountController>>().Object);

        context.ChangeTracker.Clear();
        var result = await controller.GetMySalaries(default);

        Assert.Equal(2, result.Count);
        Assert.Equal(salary1.Value, result[0].Value);
        Assert.Equal(salary2.Value, result[1].Value);
    }

    [Fact]
    public async Task GetMySalaries_OtherUser_OtherUserProfile()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new UserFake(Role.Interviewer)
            .PleaseAsync(context);

        var user2 = await new UserFake(Role.Admin)
            .PleaseAsync(context);

        var salary1 = await new UserSalaryFake(user1, 400_000)
            .PleaseAsync(context);

        var salary2 = await new UserSalaryFake(user1, 500_000)
            .PleaseAsync(context);

        var controller = new AccountController(
            new FakeAuth(user2),
            context,
            new Mock<ILogger<AccountController>>().Object);

        context.ChangeTracker.Clear();
        var result = await controller.GetMySalaries(default);

        Assert.Empty(result);
    }
}
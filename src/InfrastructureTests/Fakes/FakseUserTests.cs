using System.Threading.Tasks;
using Domain.Enums;
using TestUtils.Db;
using TestUtils.Fakes;
using Xunit;

namespace InfrastructureTests.Fakes;

public class FakseUserTests
{
    [Fact]
    public async Task PleaseAsync_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        Assert.Contains(user.UserRoles, x => x.RoleId == Role.Interviewer);
    }
}